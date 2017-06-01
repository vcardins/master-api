using System;
using System.Data;
using MasterApi.Core.Data.UnitOfWork;

namespace MasterApi.Data.EF7
{
    public static class IsolationLevelHelper
    {
        public static IsolationLevel ConvertLevel(this DbIsolationLevel level)
        {
            switch (level)
            {
                case DbIsolationLevel.Chaos: return IsolationLevel.Chaos;
                case DbIsolationLevel.ReadCommitted : return IsolationLevel.ReadCommitted;
                case DbIsolationLevel.ReadUncommitted : return IsolationLevel.ReadUncommitted;
                case DbIsolationLevel.RepeatableRead : return IsolationLevel.RepeatableRead;
                case DbIsolationLevel.Serializable : return IsolationLevel.Serializable;
                case DbIsolationLevel.Snapshot : return IsolationLevel.Snapshot;
                case DbIsolationLevel.Unspecified: return IsolationLevel.Unspecified;
                default:
                    return IsolationLevel.Unspecified;
            }
        }

        public static DbIsolationLevel ConvertLevel(this IsolationLevel level)
        {
            switch (level)
            {
                case IsolationLevel.Chaos: return DbIsolationLevel.Chaos;
                case IsolationLevel.ReadCommitted: return DbIsolationLevel.ReadCommitted;
                case IsolationLevel.ReadUncommitted: return DbIsolationLevel.ReadUncommitted;
                case IsolationLevel.RepeatableRead: return DbIsolationLevel.RepeatableRead;
                case IsolationLevel.Serializable: return DbIsolationLevel.Serializable;
                case IsolationLevel.Snapshot: return DbIsolationLevel.Snapshot;
                case IsolationLevel.Unspecified: return DbIsolationLevel.Unspecified;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level));
            }
        }
    }
}