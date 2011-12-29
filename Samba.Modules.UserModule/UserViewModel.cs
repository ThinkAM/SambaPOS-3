﻿using System;
using System.Collections.Generic;
using FluentValidation;
using Samba.Domain.Models.Users;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;
using System.Linq;
using Samba.Services.Common;

namespace Samba.Modules.UserModule
{
    public class UserViewModel : EntityViewModelBase<User>
    {
        private bool _edited;

        public UserViewModel()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<UserRole>>().Subscribe(x => RaisePropertyChanged(() => Roles));
        }

        public string PinCode
        {
            get
            {
                if (_edited) return Model.PinCode;
                return !string.IsNullOrEmpty(Model.PinCode) ? "********" : "";
            }
            set
            {
                if (Model.PinCode == null || !Model.PinCode.Contains("*") && !string.IsNullOrEmpty(value))
                {
                    _edited = true;
                    Model.PinCode = value;
                    RaisePropertyChanged(() => PinCode);
                }
            }
        }

        public UserRole Role { get { return Model.UserRole; } set { Model.UserRole = value; } }

        public IEnumerable<UserRole> Roles { get; private set; }

        public override Type GetViewType()
        {
            return typeof(UserView);
        }

        public override string GetModelTypeString()
        {
            return Resources.User;
        }

        protected override void Initialize()
        {
            Roles = Workspace.All<UserRole>();
        }

        protected override string GetSaveErrorMessage()
        {
            var users = Dao.Query<User>(x => x.PinCode == Model.PinCode);
            return users.Count() > 1 || (users.Count() == 1 && users.ElementAt(0).Id != Model.Id)
                ? Resources.SaveErrorThisPinCodeInUse : "";
        }

        protected override AbstractValidator<User> GetValidator()
        {
            return new UserValidator();
        }
    }

    internal class UserValidator : EntityValidator<User>
    {
        public UserValidator()
        {
            RuleFor(x => x.PinCode).Length(4, 10);
            RuleFor(x => x.UserRole).NotNull();
        }
    }
}