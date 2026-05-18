using System;
using Xunit;
using CrewBackend.Models;
using CrewBackend.Services;
using CrewBackend.Services.Interfaces;

namespace CrewBackend.Tests.Services
{
    public class RbacPolicyEvaluatorTests
    {
        private readonly IRbacPolicyEvaluator _evaluator = new RbacPolicyEvaluator();

        private User CreateUser(int id, string roleName)
        {
            return new User
            {
                Id = id,
                Name = $"User{id}",
                Email = $"user{id}@test.com",
                Password = "hashedpassword",
                Role = new UserRole { RoleId = id, RoleName = roleName }
            };
        }

        [Theory]
        [InlineData(UserRoleConstants.SuperAdmin, UserRoleConstants.SuperAdmin, true)]
        [InlineData(UserRoleConstants.SuperAdmin, UserRoleConstants.Admin, true)]
        [InlineData(UserRoleConstants.SuperAdmin, UserRoleConstants.Employee, true)]
        [InlineData(UserRoleConstants.Admin, UserRoleConstants.Employee, true)]
        [InlineData(UserRoleConstants.Admin, UserRoleConstants.Admin, false)]
        [InlineData(UserRoleConstants.Admin, UserRoleConstants.SuperAdmin, false)]
        [InlineData(UserRoleConstants.Employee, UserRoleConstants.Employee, false)]
        public void CanCreate_ShouldMatchPolicy(string actorRole, string targetRole, bool expected)
        {
            var actor = CreateUser(1, actorRole);
            var result = _evaluator.CanCreate(actor, targetRole);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(UserRoleConstants.SuperAdmin, UserRoleConstants.SuperAdmin, true)]
        [InlineData(UserRoleConstants.SuperAdmin, UserRoleConstants.Admin, true)]
        [InlineData(UserRoleConstants.SuperAdmin, UserRoleConstants.Employee, true)]
        [InlineData(UserRoleConstants.Admin, UserRoleConstants.Employee, true)]
        [InlineData(UserRoleConstants.Admin, UserRoleConstants.SuperAdmin, false)]
        [InlineData(UserRoleConstants.Admin, UserRoleConstants.Admin, false)]
        [InlineData(UserRoleConstants.Employee, UserRoleConstants.SuperAdmin, false)]
        [InlineData(UserRoleConstants.Employee, UserRoleConstants.Admin, false)]
        [InlineData(UserRoleConstants.Employee, UserRoleConstants.Employee, false)]
        public void CanUpdate_OtherUser_ShouldMatchPolicy(string actorRole, string targetRole, bool expected)
        {
            var actor = CreateUser(1, actorRole);
            var target = CreateUser(2, targetRole);
            var result = _evaluator.CanUpdate(actor, target);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(UserRoleConstants.SuperAdmin)]
        [InlineData(UserRoleConstants.Admin)]
        [InlineData(UserRoleConstants.Employee)]
        public void CanUpdate_Self_ShouldAlwaysBeTrue(string roleName)
        {
            var user = CreateUser(1, roleName);
            var result = _evaluator.CanUpdate(user, user);
            Assert.True(result);
        }

        [Theory]
        [InlineData(UserRoleConstants.SuperAdmin, UserRoleConstants.SuperAdmin, true)]
        [InlineData(UserRoleConstants.SuperAdmin, UserRoleConstants.Admin, true)]
        [InlineData(UserRoleConstants.SuperAdmin, UserRoleConstants.Employee, true)]
        [InlineData(UserRoleConstants.Admin, UserRoleConstants.Employee, true)]
        [InlineData(UserRoleConstants.Admin, UserRoleConstants.Admin, false)]
        [InlineData(UserRoleConstants.Admin, UserRoleConstants.SuperAdmin, false)]
        [InlineData(UserRoleConstants.Employee, UserRoleConstants.SuperAdmin, false)]
        [InlineData(UserRoleConstants.Employee, UserRoleConstants.Admin, false)]
        [InlineData(UserRoleConstants.Employee, UserRoleConstants.Employee, false)]
        public void CanDelete_OtherUser_ShouldMatchPolicy(string actorRole, string targetRole, bool expected)
        {
            var actor = CreateUser(1, actorRole);
            var target = CreateUser(2, targetRole);
            var result = _evaluator.CanDelete(actor, target);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(UserRoleConstants.SuperAdmin)]
        [InlineData(UserRoleConstants.Admin)]
        [InlineData(UserRoleConstants.Employee)]
        public void CanDelete_Self_ShouldAlwaysBeFalse(string roleName)
        {
            var user = CreateUser(1, roleName);
            var result = _evaluator.CanDelete(user, user);
            Assert.False(result);
        }

        // ===============================
        // Organisation RBAC Tests
        // ===============================

        [Theory]
        [InlineData(UserRoleConstants.SuperAdmin, true)]
        [InlineData(UserRoleConstants.Admin, true)]
        [InlineData(UserRoleConstants.Employee, false)]
        public void CanCreateOrganisation_ShouldMatchPolicy(string actorRole, bool expected)
        {
            var actor = CreateUser(1, actorRole);
            var result = _evaluator.CanCreateOrganisation(actor);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(UserRoleConstants.SuperAdmin, true)]
        [InlineData(UserRoleConstants.Admin, true)]
        [InlineData(UserRoleConstants.Employee, false)]
        public void CanUpdateOrganisation_ShouldMatchPolicy(string actorRole, bool expected)
        {
            var actor = CreateUser(1, actorRole);
            var result = _evaluator.CanUpdateOrganisation(actor);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(UserRoleConstants.SuperAdmin, true)]
        [InlineData(UserRoleConstants.Admin, true)]
        [InlineData(UserRoleConstants.Employee, false)]
        public void CanDeleteOrganisation_ShouldMatchPolicy(string actorRole, bool expected)
        {
            var actor = CreateUser(1, actorRole);
            var result = _evaluator.CanDeleteOrganisation(actor);
            Assert.Equal(expected, result);
        }
    }
}
