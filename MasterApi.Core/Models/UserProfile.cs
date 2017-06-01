using System;
using MasterApi.Core.Account.Models;

namespace MasterApi.Core.Models
{
    public class UserProfile : BaseObjectState
    {
        public virtual int UserId { get; set; }
        public virtual UserAccount UserAccount { get; set; }

        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string DisplayName { get; set; }
        public string Avatar { get; set; }

        //public string ApartmentUnitNumber { get; set; }
        //public string StreetNumber { get; set; }
        //public string StreetAddress { get; set; }
        //public string StreetAddress2 { get; set; }
        //public string City { get; set; }
        //public string DistrictRegion { get; set; }

        public string City { get; set; }
        public string Iso2 { get; set; }
        public string ProvinceState { get; set; }
        public ProvinceState ResidenceProvinceState { get; set; }

        public virtual DateTimeOffset Created { get; set; }

    }
}

