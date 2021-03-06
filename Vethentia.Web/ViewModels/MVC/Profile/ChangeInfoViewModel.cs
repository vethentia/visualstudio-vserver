﻿namespace Keysme.Web.ViewModels.Profile
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using Automapper;

    using Data.Models;

    using Global;

    public class ChangeInfoViewModel : IMapTo<User>, IMapFrom<User>
    {
        private DateTime birthDate;

        [Required]
        [MaxLength(ValidationConstants.UserFirstNameMaxLength)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(ValidationConstants.UserLastNameMaxLength)]
        public string LastName { get; set; }

        public Gender Gender { get; set; }

        public DateTime BirthDate
        {
            get
            {
                return this.birthDate;
            }
            set
            {
                this.birthDate = value;
                this.BirthDay = value.Day;
                this.BirthMonth = value.Month;
                this.BirthYear = value.Year;
            }
        }

        public int BirthDay { get; set; }

        public int BirthYear { get; set; }

        public int BirthMonth { get; set; }

        [MaxLength(ValidationConstants.UserAboutMaxLength)]
        public string About { get; set; }

        [MaxLength(ValidationConstants.UserLocationMaxLength)]
        public string Location { get; set; }

        [MaxLength(ValidationConstants.UserSchoolMaxLength)]
        public string School { get; set; }

        [MaxLength(ValidationConstants.UserWorkMaxLength)]
        public string Work { get; set; }

        [MaxLength(ValidationConstants.UserWorkMaxLength)]
        public string Languages { get; set; }

        [MaxLength(ValidationConstants.UserCommentMaxLength)]
        public string Comment { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        public PhoneNumberCountryCode PhoneNumberCountryCode { get; set; }
        public string ProfileImage { get; set; }
    }
}