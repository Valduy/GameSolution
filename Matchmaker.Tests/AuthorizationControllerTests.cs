using System;
using System.Collections;
using System.Collections.Generic;
using Matchmaker.Controllers;
using Matchmaker.Services.Implementations;
using Matchmaker.Services.Interfaces;
using Xunit;

namespace Matchmaker.Tests
{
    public class AuthorizationControllerTests
    {
        [Theory]
        [ClassData(typeof(TestAuthorizationServices))]
        public void AuthorizationTheory(ISimpleAuthorizationService authorizationService)
        {
            var controller = new AuthorizationController(authorizationService);

        }
    }

    public class TestAuthorizationServices : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] {new SimpleAuthorizationService()};
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
