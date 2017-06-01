using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MasterApi.Core.Account.Models;
using MasterApi.Core.Account.Events;
using System.Linq;
using System.Collections.Generic;
using Omu.ValueInjecter;
using MasterApi.Core.Data.Infrastructure;
using MasterApi.Core.Account;
using MasterApi.Core.Account.ViewModels;

namespace MasterApi.Services.Account
{
    public partial class UserAccountService 
    {

        public async Task<IEnumerable<PasswordResetSecretOutput>> GetSecretQuestionsAsync(int accountId)
        {
            var account = await GetByIdAsync(accountId, x => x.PasswordResetSecretCollection);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            var secrets = account.PasswordResetSecretCollection.Select(x => new PasswordResetSecretOutput().InjectFrom(x)).Cast<PasswordResetSecretOutput>();
            return secrets;
        }

        public async Task AddPasswordResetSecretAsync(int accountId, string question, string answer)
        {
            _logger.LogInformation(GetLogMessage($"called: {accountId}"));

            if (string.IsNullOrWhiteSpace(question))
            {
                _logger.LogError(GetLogMessage("failed -- null question"));
                throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.SecretQuestionRequired));
            }
            if (string.IsNullOrWhiteSpace(answer))
            {
                _logger.LogError(GetLogMessage("failed -- null answer"));
                throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.SecretAnswerRequired));
            }

            var account = await GetByIdAsync(accountId, x => x.PasswordResetSecretCollection);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            if (account.PasswordResetSecretCollection.Any(x => x.Question == question))
            {
                _logger.LogError(GetLogMessage("failed -- question already exists"));
                throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.SecretQuestionAlreadyInUse));
            }

            _logger.LogTrace(GetLogMessage("success"));

            var secret = new PasswordResetSecret
            {
                Guid = Guid.NewGuid(),
                Question = question,
                Answer = _crypto.Hash(answer),
                ObjectState = ObjectState.Added
            };

            account.AddPasswordResetSecret(secret);

            AddEvent(new PasswordResetSecretAddedEvent { Account = account, Secret = secret });

            Repository.InsertOrUpdateGraph(account, true);

            FireEvents();
        }

        public async Task RemovePasswordResetSecretAsync(int accountId, Guid questionId)
        {
            _logger.LogInformation(GetLogMessage($"called: Acct: {accountId}, Question: {questionId}"));

            var account = await GetByIdAsync(accountId, x => x.PasswordResetSecretCollection);
            if (account == null) throw new ArgumentException("Invalid AccountID");

            var item = account.PasswordResetSecretCollection.SingleOrDefault(x => x.Guid == questionId);
            if (item != null)
            {
                _logger.LogTrace(GetLogMessage("success -- item removed"));

                account.RemovePasswordResetSecret(item);
                AddEvent(new PasswordResetSecretRemovedEvent { Account = account, Secret = item });
                Update(account);
            }
            else
            {
                _logger.LogTrace(GetLogMessage("no matching item found"));
            }
        }

        public async Task ResetPasswordFromSecretQuestionAndAnswerAsync(Guid accountGuid, PasswordResetQuestionAnswer[] answers)
        {
            _logger.LogInformation(GetLogMessage($"called: {accountGuid}"));

            if (answers == null || answers.Length == 0 || answers.Any(x => string.IsNullOrWhiteSpace(x.Answer)))
            {
                _logger.LogError(GetLogMessage("failed -- no answers"));
                throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.SecretAnswerRequired));
            }

            var account = await GetByGuidAsync(accountGuid, x => x.PasswordResetSecretCollection);
            if (account == null)
            {
                _logger.LogError(GetLogMessage("failed -- invalid account id"));
                throw new Exception("Invalid Account ID");
            }

            if (string.IsNullOrWhiteSpace(account.Email))
            {
                _logger.LogError(GetLogMessage("no email to use for password reset"));
                throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.PasswordResetErrorNoEmail));
            }

            if (!account.PasswordResetSecretCollection.Any())
            {
                _logger.LogError(GetLogMessage("failed -- account not configured for secret question/answer"));
                throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.AccountNotConfiguredWithSecretQuestion));
            }

            if (account.FailedPasswordResetCount >= Settings.AccountLockoutFailedLoginAttempts &&
                account.LastFailedPasswordReset >= UtcNow.Subtract(Settings.AccountLockoutDuration))
            {
                account.FailedPasswordResetCount++;

                AddEvent(new PasswordResetFailedEvent { Account = account });

                Update(account, true);

                _logger.LogError(GetLogMessage("failed -- too many failed password reset attempts"));
                throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.InvalidQuestionOrAnswer));
            }

            var secrets = account.PasswordResetSecretCollection.ToArray();
            var failed = false;
            foreach (var answer in answers)
            {
                var secret = secrets.SingleOrDefault(x => x.Guid == answer.QuestionId);
                if (secret != null && _crypto.VerifyHash(answer.Answer, secret.Answer)) continue;
                _logger.LogError(GetLogMessage($"failed on question id: {answer.QuestionId}"));
                failed = true;
            }

            if (failed)
            {
                account.LastFailedPasswordReset = UtcNow;
                if (account.FailedPasswordResetCount <= 0)
                {
                    account.FailedPasswordResetCount = 1;
                }
                else
                {
                    account.FailedPasswordResetCount++;
                }
                AddEvent(new PasswordResetFailedEvent { Account = account });
            }
            else
            {
                _logger.LogTrace(GetLogMessage("success"));

                account.LastFailedPasswordReset = null;
                account.FailedPasswordResetCount = 0;
                ResetPassword(account);
            }

            Update(account, true);

            if (failed)
            {
                throw new ValidationException(GetValidationMessage(UserAccountConstants.ValidationMessages.InvalidQuestionOrAnswer));
            }
        }
    }
}
