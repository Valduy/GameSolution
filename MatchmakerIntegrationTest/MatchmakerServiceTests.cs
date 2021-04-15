using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Matches;
using Matchmaker.Services;
using Network;
using Orderers;
using Orderers.Attributes;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Matchmaker.Tests.Integration
{
    public class MatchmakerServiceFixture : IDisposable
    {
        public MatchmakerService<ListenSessionMatch> Matchmaker { get; }
        public string UserId1 => "userId1";
        public string UserId2 => "userId2";

        public MatchmakerServiceFixture()
        {
            Matchmaker = new MatchmakerService<ListenSessionMatch>(2);
        }

        public void Dispose()
        {
            Matchmaker.Dispose();
        }
    }

    public class MatchmakerServiceTestsOrder : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            throw new NotImplementedException();
        }
    }

    [TestCaseOrderer(nameof(PriorityOrderer), "Orderers")]
    public class MatchmakerServiceTests : IClassFixture<MatchmakerServiceFixture>
    {
        private readonly MatchmakerServiceFixture _fixture;

        public MatchmakerServiceTests(MatchmakerServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact, TestPriority(0)]
        public void GetStatus_UserId_ReturnUserStatusAbsent()
        {
            // Arrange
            var userId = "userId";

            // Act
            var status = _fixture.Matchmaker.GetStatus(userId);

            // Assert
            Assert.Equal(UserStatus.Absent, status);
        }

        [Fact, TestPriority(1)]
        public void Enqueue_2UsersId_ReturnUsersStatusesWait()
        {
            // Arrange

            // Act
            _fixture.Matchmaker.Enqueue(_fixture.UserId1);
            _fixture.Matchmaker.Enqueue(_fixture.UserId2);
            var status1 = _fixture.Matchmaker.GetStatus(_fixture.UserId1);
            var status2 = _fixture.Matchmaker.GetStatus(_fixture.UserId2);

            // Assert
            Assert.Equal(UserStatus.Wait, status1);
            Assert.Equal(UserStatus.Wait, status2);
        }
    }
}
