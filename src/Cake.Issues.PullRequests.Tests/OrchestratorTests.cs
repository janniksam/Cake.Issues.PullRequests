namespace Cake.Issues.PullRequests.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Cake.Core.IO;
    using Cake.Issues.Testing;
    using Shouldly;
    using Xunit;

    public sealed class OrchestratorTests
    {
        public sealed class TheCtorWithIssues
        {
            [Fact]
            public void Should_Throw_If_Log_Is_Null()
            {
                // Given
                var fixture = new PullRequestsFixture
                {
                    Log = null
                };
                var issues = new List<IIssue>();

                // When
                var result = Record.Exception(() => fixture.RunOrchestratorForIssues(issues));

                // Then
                result.IsArgumentNullException("log");
            }

            [Fact]
            public void Should_Throw_If_Issues_Are_Null()
            {
                // Given
                var fixture = new PullRequestsFixture();
                List<IIssue> issues = null;

                // When
                var result = Record.Exception(() => fixture.RunOrchestratorForIssues(issues));

                // Then
                result.IsArgumentNullException("issues");
            }

            [Fact]
            public void Should_Throw_If_Issue_Is_Null()
            {
                // Given
                var fixture = new PullRequestsFixture();
                var issues = new List<IIssue> { null };

                // When
                var result = Record.Exception(() => fixture.RunOrchestratorForIssues(issues));

                // Then
                result.IsArgumentOutOfRangeException("issues");
            }

            [Fact]
            public void Should_Throw_If_Pull_Request_System_Is_Null()
            {
                // Given
                var fixture = new PullRequestsFixture
                {
                    PullRequestSystem = null
                };
                var issues = new List<IIssue>();

                // When
                var result = Record.Exception(() => fixture.RunOrchestratorForIssues(issues));

                // Then
                result.IsArgumentNullException("pullRequestSystem");
            }

            [Fact]
            public void Should_Throw_If_Settings_Are_Null()
            {
                // Given
                var fixture = new PullRequestsFixture
                {
                    Settings = null
                };
                var issues = new List<IIssue>();

                // When
                var result = Record.Exception(() => fixture.RunOrchestratorForIssues(issues));

                // Then
                result.IsArgumentNullException("settings");
            }

            [Fact]
            public void Should_Initialize_Pull_Request_System()
            {
                // Given
                var fixture = new PullRequestsFixture();
                var issues = new List<IIssue>();

                // When
                fixture.RunOrchestratorForIssues(issues);

                // Then
                fixture.PullRequestSystem.Settings.ShouldBe(fixture.Settings);
            }

            [Fact]
            public void Should_Not_Throw_If_Issues_Are_Empry()
            {
                // Given
                var fixture = new PullRequestsFixture();
                var issues = new List<IIssue>();

                // When
                var result = fixture.RunOrchestratorForIssues(issues);

                // Then
                result.ReportedIssues.ShouldBeEmpty();
                result.PostedIssues.ShouldBeEmpty();
            }
        }

        public sealed class TheCtorWithIssueProviders
        {
            [Fact]
            public void Should_Throw_If_Log_Is_Null()
            {
                // Given
                var fixture = new PullRequestsFixture
                {
                    Log = null
                };

                // When
                var result = Record.Exception(() => fixture.RunOrchestratorForIssueProviders());

                // Then
                result.IsArgumentNullException("log");
            }

            [Fact]
            public void Should_Throw_If_Issue_Provider_List_Is_Null()
            {
                // Given
                var fixture = new PullRequestsFixture
                {
                    IssueProviders = null
                };

                // When
                var result = Record.Exception(() => fixture.RunOrchestratorForIssueProviders());

                // Then
                result.IsArgumentNullException("issueProviders");
            }

            [Fact]
            public void Should_Throw_If_Issue_Provider_List_Is_Empty()
            {
                // Given
                var fixture = new PullRequestsFixture();
                fixture.IssueProviders.Clear();

                // When
                var result = Record.Exception(() => fixture.RunOrchestratorForIssueProviders());

                // Then
                result.IsArgumentException("issueProviders");
            }

            [Fact]
            public void Should_Throw_If_Issue_Provider_Is_Null()
            {
                // Given
                var fixture = new PullRequestsFixture();
                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(null);

                // When
                var result = Record.Exception(() => fixture.RunOrchestratorForIssueProviders());

                // Then
                result.IsArgumentOutOfRangeException("issueProviders");
            }

            [Fact]
            public void Should_Throw_If_Pull_Request_System_Is_Null()
            {
                // Given
                var fixture = new PullRequestsFixture
                {
                    PullRequestSystem = null
                };

                // When
                var result = Record.Exception(() => fixture.RunOrchestratorForIssueProviders());

                // Then
                result.IsArgumentNullException("pullRequestSystem");
            }

            [Fact]
            public void Should_Throw_If_Settings_Are_Null()
            {
                // Given
                var fixture = new PullRequestsFixture
                {
                    Settings = null
                };

                // When
                var result = Record.Exception(() => fixture.RunOrchestratorForIssueProviders());

                // Then
                result.IsArgumentNullException("settings");
            }

            [Fact]
            public void Should_Initialize_Pull_Request_System()
            {
                // Given
                var fixture = new PullRequestsFixture();

                // When
                fixture.RunOrchestratorForIssueProviders();

                // Then
                fixture.PullRequestSystem.Settings.ShouldBe(fixture.Settings);
            }
        }

        public sealed class TheRunMethod
        {
            [Fact]
            public void Should_Post_Issue()
            {
                // Given
                var issueToPost =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture = new PullRequestsFixture();

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issueToPost
                        }));

                // When
                fixture.RunOrchestratorForIssueProviders();

                // Then
                fixture.PullRequestSystem.PostedIssues.ShouldContain(issueToPost);
                fixture.Log.Entries.ShouldContain(
                    x =>
                        x.Message ==
                            $"Posting 1 issue(s):\n  Rule: {issueToPost.Rule} Line: {issueToPost.Line} File: {issueToPost.AffectedFileRelativePath}");
            }

            [Fact]
            public void Should_Post_Issue_Not_Related_To_A_File()
            {
                // Given
                var issueToPost =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture = new PullRequestsFixture();

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issueToPost
                        }));

                // When
                fixture.RunOrchestratorForIssueProviders();

                // Then
                fixture.PullRequestSystem.PostedIssues.ShouldContain(issueToPost);
                fixture.Log.Entries.ShouldContain(
                    x =>
                        x.Message ==
                            $"Posting 1 issue(s):\n  Rule: {issueToPost.Rule} Line: {issueToPost.Line} File: {issueToPost.AffectedFileRelativePath}");
            }

            [Fact]
            public void Should_Return_Correct_Values()
            {
                // Given
                var reportedIssue =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var postedIssue =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture = new PullRequestsFixture();

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            postedIssue, reportedIssue
                        }));

                fixture.ReportIssuesToPullRequestSettings.MaxIssuesToPost = 1;

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                result.ReportedIssues.Count().ShouldBe(2);
                result.ReportedIssues.ShouldContain(reportedIssue);
                result.ReportedIssues.ShouldContain(postedIssue);
                result.PostedIssues.Count().ShouldBe(1);
                result.PostedIssues.ShouldContain(postedIssue);
            }

            [Fact]
            public void Should_Return_Reported_Issues_If_PullRequestSystem_Could_Not_Be_Initialized()
            {
                // Given
                var firstIssue =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var secondIssue =
                     IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture = new PullRequestsFixture();
                fixture.PullRequestSystem.ShouldFailOnInitialization = true;

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            firstIssue, secondIssue
                        }));

                fixture.ReportIssuesToPullRequestSettings.MaxIssuesToPost = 1;

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                result.ReportedIssues.Count().ShouldBe(2);
                result.ReportedIssues.ShouldContain(firstIssue);
                result.ReportedIssues.ShouldContain(secondIssue);
                result.PostedIssues.ShouldBeEmpty();
            }

            [Fact]
            public void Should_Limit_Messages_To_Global_Maximum()
            {
                // Given
                var issue1 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue2 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderType Bar", "ProviderName Bar")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 12)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture = new PullRequestsFixture();

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issue1, issue2
                        }));

                fixture.ReportIssuesToPullRequestSettings.MaxIssuesToPost = 1;

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                result.ReportedIssues.Count().ShouldBe(2);
                result.PostedIssues.Count().ShouldBe(1);
                result.PostedIssues.ShouldContain(issue1);
                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) were filtered to match the global issue limit of 1");
                fixture.Log.Entries.ShouldContain(x => x.Message.StartsWith("Posting 1 issue(s):"));
            }

            [Fact]
            public void Should_Limit_Messages_To_Global_Maximum_By_Priority()
            {
                // Given
                var issue1 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue2 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderType Bar", "ProviderName Bar")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 12)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Error)
                        .Create();

                var fixture = new PullRequestsFixture();

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issue1, issue2
                        }));

                fixture.ReportIssuesToPullRequestSettings.MaxIssuesToPost = 1;

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                result.ReportedIssues.Count().ShouldBe(2);
                result.PostedIssues.Count().ShouldBe(1);
                result.PostedIssues.ShouldContain(issue2);
                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) were filtered to match the global issue limit of 1");
                fixture.Log.Entries.ShouldContain(x => x.Message.StartsWith("Posting 1 issue(s):"));
            }

            [Fact]
            public void Should_Limit_Messages_To_Global_Maximum_By_FilePath()
            {
                // Given
                var issue1 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue2 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderType Bar", "ProviderName Bar")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 12)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture = new PullRequestsFixture();

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issue1, issue2
                        }));

                fixture.ReportIssuesToPullRequestSettings.MaxIssuesToPost = 1;

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                result.ReportedIssues.Count().ShouldBe(2);
                result.PostedIssues.Count().ShouldBe(1);
                result.PostedIssues.ShouldContain(issue2);
                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) were filtered to match the global issue limit of 1");
                fixture.Log.Entries.ShouldContain(x => x.Message.StartsWith("Posting 1 issue(s):"));
            }

            [Fact]
            public void Should_Limit_Messages_To_Maximum_Per_Issue_Provider()
            {
                // Given
                var issue1 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderTypeA", "ProviderNameA")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue2 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderTypeA", "ProviderNameA")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 12)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue3 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderTypeB", "ProviderNameB")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue4 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderTypeB", "ProviderNameB")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 12)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture = new PullRequestsFixture();

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issue1, issue2, issue3, issue4
                        }));

                fixture.ReportIssuesToPullRequestSettings.MaxIssuesToPostForEachIssueProvider = 1;

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                result.ReportedIssues.Count().ShouldBe(4);
                result.PostedIssues.Count().ShouldBe(2);
                result.PostedIssues.ShouldContain(issue1);
                result.PostedIssues.ShouldContain(issue3);
                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) of type ProviderTypeA were filtered to match the maximum of 1 issues which should be reported for each issue provider");
                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) of type ProviderTypeB were filtered to match the maximum of 1 issues which should be reported for each issue provider");
                fixture.Log.Entries.ShouldContain(x => x.Message.StartsWith("Posting 2 issue(s):"));
            }

            [Fact]
            public void Should_Limit_Messages_To_Maximum_Per_Issue_Provider_By_Priority()
            {
                // Given
                var issue1 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderTypeA", "ProviderNameA")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue2 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderTypeA", "ProviderNameA")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 12)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Error)
                        .Create();
                var issue3 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderTypeB", "ProviderNameB")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Error)
                        .Create();
                var issue4 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderTypeB", "ProviderNameB")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 12)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture = new PullRequestsFixture();

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issue1, issue2, issue3, issue4
                        }));

                fixture.ReportIssuesToPullRequestSettings.MaxIssuesToPostForEachIssueProvider = 1;

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                result.ReportedIssues.Count().ShouldBe(4);
                result.PostedIssues.Count().ShouldBe(2);
                result.PostedIssues.ShouldContain(issue2);
                result.PostedIssues.ShouldContain(issue3);
                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) of type ProviderTypeA were filtered to match the maximum of 1 issues which should be reported for each issue provider");
                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) of type ProviderTypeB were filtered to match the maximum of 1 issues which should be reported for each issue provider");
                fixture.Log.Entries.ShouldContain(x => x.Message.StartsWith("Posting 2 issue(s):"));
            }

            [Fact]
            public void Should_Limit_Messages_To_Maximum_Per_Issue_Provider_By_FilePath()
            {
                // Given
                var issue1 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderTypeA", "ProviderNameA")
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue2 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderTypeA", "ProviderNameA")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 12)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue3 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderTypeB", "ProviderNameB")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue4 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderTypeB", "ProviderNameB")
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture = new PullRequestsFixture();

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issue1, issue2, issue3, issue4
                        }));

                fixture.ReportIssuesToPullRequestSettings.MaxIssuesToPostForEachIssueProvider = 1;

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                result.ReportedIssues.Count().ShouldBe(4);
                result.PostedIssues.Count().ShouldBe(2);
                result.PostedIssues.ShouldContain(issue2);
                result.PostedIssues.ShouldContain(issue3);
                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) of type ProviderTypeA were filtered to match the maximum of 1 issues which should be reported for each issue provider");
                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) of type ProviderTypeB were filtered to match the maximum of 1 issues which should be reported for each issue provider");
                fixture.Log.Entries.ShouldContain(x => x.Message.StartsWith("Posting 2 issue(s):"));
            }
        }

        public sealed class TheRunMethodWithCheckingCommitIdCapability
        {
            [Fact]
            public void Should_Skip_Posting_If_Commit_Is_Outdate()
            {
                // Given
                var issueToPost =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture =
                    new PullRequestsFixture(
                        (builder, settings) => builder
                            .WithCheckingCommitIdCapability());

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issueToPost
                        }));

                fixture.PullRequestSystem.CheckingCommitIdCapability.LastSourceCommitId =
                    "9ebcec39e16c39b5ffcb10f253d0c2bcf8438cf6";
                fixture.ReportIssuesToPullRequestSettings.CommitId =
                    "15c54be6435cfb6b6973896d7be79f1d9b7497a9";

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                fixture.PullRequestSystem.PostedIssues.ShouldBeEmpty();
                result.PostedIssues.ShouldBeEmpty();
            }
        }

        public sealed class TheRunMethodWithDiscussionThreadsCapability
        {
            [Fact]
            public void Should_Limit_Messages_To_Maximum_Across_Runs_Nothing_New_To_Post()
            {
                // Given
                var issue1 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue2 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderType Bar", "ProviderName Bar")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 12)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue3 =
                    IssueBuilder
                        .NewIssue("Message BarFoo", "ProviderType Bar", "ProviderName Bar")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 13)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture =
                    new PullRequestsFixture(
                        (builder, settings) => builder
                            .WithDiscussionThreadsCapability(
                                new List<IPullRequestDiscussionThread>
                                {
                                    new PullRequestDiscussionThread(
                                        1,
                                        PullRequestDiscussionStatus.Active,
                                        @"src\Cake.Issues.Tests\FakeIssueProvider.cs",
                                        new List<IPullRequestDiscussionComment>
                                        {
                                            new PullRequestDiscussionComment
                                            {
                                                Content = "Message Foo",
                                                IsDeleted = false
                                            }
                                        }),
                                    new PullRequestDiscussionThread(
                                        2,
                                        PullRequestDiscussionStatus.Active,
                                        @"src\Cake.Issues.Tests\FakeIssueProvider.cs",
                                        new List<IPullRequestDiscussionComment>
                                        {
                                            new PullRequestDiscussionComment
                                            {
                                                Content = "Message Bar",
                                                IsDeleted = false
                                            },
                                        })
                                }));

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issue1, issue2, issue3
                        }));

                fixture.ReportIssuesToPullRequestSettings.MaxIssuesToPostAcrossRuns = 2;

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                result.ReportedIssues.Count().ShouldBe(3);
                result.PostedIssues.Count().ShouldBe(0);
                fixture.Log.Entries.ShouldContain(x => x.Message == "2 issue(s) were filtered because they were already present");
                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) were filtered to match the global issue limit of 2 across all runs (2 issues already posted in previous runs)");
                fixture.Log.Entries.ShouldContain(x => x.Message == "All issues were filtered. Nothing new to post.");
            }

            [Fact]
            public void Should_Limit_Messages_To_Maximum_Across_Runs()
            {
                // Given
                var issue1 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue2 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderType Bar", "ProviderName Bar")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 12)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture =
                    new PullRequestsFixture(
                        (builder, settings) => builder
                            .WithDiscussionThreadsCapability(
                                new List<IPullRequestDiscussionThread>
                                {
                                    new PullRequestDiscussionThread(
                                        1,
                                        PullRequestDiscussionStatus.Active,
                                        @"src\Cake.Issues.Tests\FakeIssueProvider.cs",
                                        new List<IPullRequestDiscussionComment>
                                        {
                                            new PullRequestDiscussionComment
                                            {
                                                Content = "Message FooBar",
                                                IsDeleted = false
                                            }
                                        })
                                }));

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issue1, issue2
                        }));

                fixture.ReportIssuesToPullRequestSettings.MaxIssuesToPostAcrossRuns = 2;

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                result.ReportedIssues.Count().ShouldBe(2);
                result.PostedIssues.Count().ShouldBe(1);
                result.PostedIssues.ShouldContain(issue1);
                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) were filtered to match the global issue limit of 2 across all runs (1 issues already posted in previous runs)");
                fixture.Log.Entries.ShouldContain(x => x.Message.StartsWith("Posting 1 issue(s):"));
            }

            [Fact]
            public void Should_Limit_Messages_To_Maximum_Across_Runs_By_Priority()
            {
                // Given
                var issue1 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue2 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderType Bar", "ProviderName Bar")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 12)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Error)
                        .Create();

                var fixture =
                    new PullRequestsFixture(
                        (builder, settings) => builder
                            .WithDiscussionThreadsCapability(
                                new List<IPullRequestDiscussionThread>
                                {
                                    new PullRequestDiscussionThread(
                                        1,
                                        PullRequestDiscussionStatus.Active,
                                        @"src\Cake.Issues.Tests\FakeIssueProvider.cs",
                                        new List<IPullRequestDiscussionComment>
                                        {
                                            new PullRequestDiscussionComment
                                            {
                                                Content = "Message FooBar",
                                                IsDeleted = false
                                            }
                                        })
                                }));

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issue1, issue2
                        }));

                fixture.ReportIssuesToPullRequestSettings.MaxIssuesToPostAcrossRuns = 2;

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                result.ReportedIssues.Count().ShouldBe(2);
                result.PostedIssues.Count().ShouldBe(1);
                result.PostedIssues.ShouldContain(issue2);
                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) were filtered to match the global issue limit of 2 across all runs (1 issues already posted in previous runs)");
                fixture.Log.Entries.ShouldContain(x => x.Message.StartsWith("Posting 1 issue(s):"));
            }

            [Fact]
            public void Should_Limit_Messages_To_Maximum_Across_Runs_By_FilePath()
            {
                // Given
                var fixture = new PullRequestsFixture();
                fixture.ReportIssuesToPullRequestSettings.MaxIssuesToPostAcrossRuns = 2;

                var issue1 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue2 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderType Bar", "ProviderName Bar")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 12)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                // When
                var issues =
                    fixture.FilterIssues(
                        new List<IIssue>
                        {
                                issue1, issue2
                        },
                        new Dictionary<IIssue, IssueCommentInfo>(),
                        new List<IPullRequestDiscussionThread>
                        {
                                new PullRequestDiscussionThread(
                                    1,
                                    PullRequestDiscussionStatus.Active,
                                    @"src\Cake.Issues.Tests\FakeIssueProvider.cs",
                                    new List<IPullRequestDiscussionComment>
                                    {
                                        new PullRequestDiscussionComment
                                        {
                                            Content = "Message FooBar",
                                            IsDeleted = false
                                        }
                                    })
                        });

                // Then
                issues.Count().ShouldBe(1);
                issues.ShouldContain(issue2);
                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) were filtered to match the global issue limit of 2 across all runs (1 issues already posted in previous runs)");
            }

            [Fact]
            public void Should_Ignore_Issues_Already_Present_In_Active_Comment()
            {
                // Given
                var issue1 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue2 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderType Bar", "ProviderName Bar")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture =
                    new PullRequestsFixture(
                        (builder, settings) => builder
                            .WithDiscussionThreadsCapability(
                                new List<IPullRequestDiscussionThread>
                                {
                                    new PullRequestDiscussionThread(
                                        1,
                                        PullRequestDiscussionStatus.Active,
                                        new FilePath(@"src\Cake.Issues.Tests\FakeIssueProvider.cs"),
                                        new List<IPullRequestDiscussionComment>
                                        {
                                            new PullRequestDiscussionComment()
                                            {
                                                Content = "Message Foo",
                                                IsDeleted = false
                                            }
                                        })
                                    {
                                        CommentSource = settings.CommentSource,
                                    }
                                }));

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issue1, issue2
                        }));

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                result.ReportedIssues.Count().ShouldBe(2);
                result.ReportedIssues.ShouldContain(issue1);
                result.ReportedIssues.ShouldContain(issue2);
                result.PostedIssues.Count().ShouldBe(1);
                result.PostedIssues.ShouldContain(issue2);

                fixture.PullRequestSystem.PostedIssues.Count().ShouldBe(1);
                fixture.PullRequestSystem.PostedIssues.ShouldContain(issue2);
                fixture.PullRequestSystem.DiscussionThreadsCapability.ResolvedThreads.ShouldBeEmpty();
                fixture.PullRequestSystem.DiscussionThreadsCapability.ReopenedThreads.ShouldBeEmpty();

                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) were filtered because they were already present");
                fixture.Log.Entries.ShouldContain(x => x.Message.StartsWith("Posting 1 issue(s):"));
            }

            [Fact]
            public void Should_Ignore_Issues_Already_Present_In_WontFix_Comment()
            {
                // Given
                var issue1 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue2 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderType Bar", "ProviderName Bar")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture =
                    new PullRequestsFixture(
                        (builder, settings) => builder
                            .WithDiscussionThreadsCapability(
                                new List<IPullRequestDiscussionThread>
                                {
                                    new PullRequestDiscussionThread(
                                        1,
                                        PullRequestDiscussionStatus.Resolved,
                                        new FilePath(@"src\Cake.Issues.Tests\FakeIssueProvider.cs"),
                                        new List<IPullRequestDiscussionComment>
                                        {
                                            new PullRequestDiscussionComment()
                                            {
                                                Content = "Message Foo",
                                                IsDeleted = false
                                            }
                                        })
                                    {
                                        CommentSource = settings.CommentSource,
                                        Resolution = PullRequestDiscussionResolution.WontFix
                                    }
                                }));

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issue1, issue2
                        }));

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                result.ReportedIssues.Count().ShouldBe(2);
                result.ReportedIssues.ShouldContain(issue1);
                result.ReportedIssues.ShouldContain(issue2);
                result.PostedIssues.Count().ShouldBe(1);
                result.PostedIssues.ShouldContain(issue2);

                fixture.PullRequestSystem.PostedIssues.Count().ShouldBe(1);
                fixture.PullRequestSystem.PostedIssues.ShouldContain(issue2);
                fixture.PullRequestSystem.DiscussionThreadsCapability.ResolvedThreads.ShouldBeEmpty();
                fixture.PullRequestSystem.DiscussionThreadsCapability.ReopenedThreads.ShouldBeEmpty();

                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) were filtered because they were already present");
                fixture.Log.Entries.ShouldContain(x => x.Message.StartsWith("Posting 1 issue(s):"));
            }

            [Fact]
            public void Should_Ignore_Issues_Already_Present_Not_Related_To_A_File()
            {
                // Given
                var issue1 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue2 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderType Bar", "ProviderName Bar")
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture =
                    new PullRequestsFixture(
                        (builder, settings) => builder
                            .WithDiscussionThreadsCapability(
                                new List<IPullRequestDiscussionThread>
                                {
                                    new PullRequestDiscussionThread(
                                        1,
                                        PullRequestDiscussionStatus.Active,
                                        null,
                                        new List<IPullRequestDiscussionComment>
                                        {
                                            new PullRequestDiscussionComment()
                                            {
                                                Content = "Message Foo",
                                                IsDeleted = false
                                            }
                                        })
                                    {
                                        CommentSource = settings.CommentSource,
                                    }
                                }));

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issue1,
                            issue2
                        }));

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                result.ReportedIssues.Count().ShouldBe(2);
                result.ReportedIssues.ShouldContain(issue1);
                result.ReportedIssues.ShouldContain(issue2);
                result.PostedIssues.Count().ShouldBe(1);
                result.PostedIssues.ShouldContain(issue2);

                fixture.PullRequestSystem.PostedIssues.Count().ShouldBe(1);
                fixture.PullRequestSystem.PostedIssues.ShouldContain(issue2);
                fixture.PullRequestSystem.DiscussionThreadsCapability.ResolvedThreads.ShouldBeEmpty();
                fixture.PullRequestSystem.DiscussionThreadsCapability.ReopenedThreads.ShouldBeEmpty();

                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) were filtered because they were already present");
                fixture.Log.Entries.ShouldContain(x => x.Message.StartsWith("Posting 1 issue(s):"));
            }

            [Fact]
            public void Should_Only_Ignore_Issues_With_Same_Comment_Source()
            {
                // Given
                var issue =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture =
                    new PullRequestsFixture(
                        (builder, settings) => builder
                            .WithDiscussionThreadsCapability(
                                new List<IPullRequestDiscussionThread>
                                {
                                    new PullRequestDiscussionThread(
                                        1,
                                        PullRequestDiscussionStatus.Active,
                                        new FilePath(@"src\Cake.Issues.Tests\FakeIssueProvider.cs"),
                                        new List<IPullRequestDiscussionComment>
                                        {
                                            new PullRequestDiscussionComment()
                                            {
                                                Content = "Foo",
                                                IsDeleted = false
                                            }
                                        })
                                    {
                                        CommentSource = "DifferentCommentSource",
                                    }
                                }));

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issue
                        }));

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                result.ReportedIssues.Count().ShouldBe(1);
                result.ReportedIssues.ShouldContain(issue);
                result.PostedIssues.Count().ShouldBe(1);
                result.PostedIssues.ShouldContain(issue);

                fixture.PullRequestSystem.PostedIssues.Count().ShouldBe(1);
                fixture.PullRequestSystem.PostedIssues.ShouldContain(issue);
                fixture.PullRequestSystem.DiscussionThreadsCapability.ResolvedThreads.ShouldBeEmpty();
                fixture.PullRequestSystem.DiscussionThreadsCapability.ReopenedThreads.ShouldBeEmpty();

                fixture.Log.Entries.ShouldContain(x => x.Message == "0 issue(s) were filtered because they were already present");
                fixture.Log.Entries.ShouldContain(x => x.Message.StartsWith("Posting 1 issue(s):"));
            }

            [Fact]
            public void Should_Resolve_Closed_Issues()
            {
                // Given
                var fixture =
                    new PullRequestsFixture(
                        (builder, settings) => builder
                            .WithDiscussionThreadsCapability(
                                new List<IPullRequestDiscussionThread>
                                {
                                    new PullRequestDiscussionThread(
                                        1,
                                        PullRequestDiscussionStatus.Active,
                                        new FilePath(@"src\Cake.Issues.Tests\FakeIssueProvider.cs"),
                                        new List<IPullRequestDiscussionComment>
                                        {
                                            new PullRequestDiscussionComment()
                                            {
                                                Content = "Bar",
                                                IsDeleted = false
                                            }
                                        })
                                    {
                                        CommentSource = settings.CommentSource
                                    }
                                }));

                var threadToResolve =
                    fixture.PullRequestSystem.DiscussionThreadsCapability.DiscussionThreads.Single();

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            IssueBuilder
                                .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                                .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                                .OfRule("Rule Foo")
                                .WithPriority(IssuePriority.Warning)
                                .Create()
                        }));

                // When
                fixture.RunOrchestratorForIssueProviders();

                // Then
                fixture.PullRequestSystem.DiscussionThreadsCapability.ResolvedThreads.ShouldContain(threadToResolve);
                fixture.Log.Entries.ShouldContain(x => x.Message == "Found 1 existing thread(s) that do not match any new issue and can be resolved.");
            }

            [Fact]
            public void Should_Only_Resolve_Issues_From_Same_Comment_Source()
            {
                // Given
                var fixture =
                    new PullRequestsFixture(
                        (builder, settings) => builder
                            .WithDiscussionThreadsCapability(
                                new List<IPullRequestDiscussionThread>
                                {
                                    new PullRequestDiscussionThread(
                                        1,
                                        PullRequestDiscussionStatus.Active,
                                        new FilePath(@"src\Cake.Issues.Tests\FakeIssueProvider.cs"),
                                        new List<IPullRequestDiscussionComment>
                                        {
                                            new PullRequestDiscussionComment()
                                            {
                                                Content = "Bar",
                                                IsDeleted = false
                                            }
                                        })
                                    {
                                        CommentSource = "DifferentCommentSource"
                                    }
                                }));

                var threadToResolve =
                    fixture.PullRequestSystem.DiscussionThreadsCapability.DiscussionThreads.Single();

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            IssueBuilder
                                .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                                .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                                .OfRule("Rule Foo")
                                .WithPriority(IssuePriority.Warning)
                                .Create()
                        }));

                // When
                fixture.RunOrchestratorForIssueProviders();

                // Then
                fixture.PullRequestSystem.DiscussionThreadsCapability.ResolvedThreads.ShouldBeEmpty();
            }

            [Fact]
            public void Should_Reopen_Still_Active_Issues()
            {
                // Given
                var fixture =
                    new PullRequestsFixture(
                        (builder, settings) => builder
                            .WithDiscussionThreadsCapability(
                                new List<IPullRequestDiscussionThread>
                                {
                                    new PullRequestDiscussionThread(
                                        1,
                                        PullRequestDiscussionStatus.Resolved,
                                        new FilePath(@"src\Cake.Issues.Tests\FakeIssueProvider.cs"),
                                        new List<IPullRequestDiscussionComment>
                                        {
                                            new PullRequestDiscussionComment()
                                            {
                                                Content = "Message Foo",
                                                IsDeleted = false
                                            }
                                        })
                                    {
                                        CommentSource = settings.CommentSource,
                                        Resolution = PullRequestDiscussionResolution.Resolved
                                    }
                                }));

                var threadToReopen =
                    fixture.PullRequestSystem.DiscussionThreadsCapability.DiscussionThreads.Single();

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            IssueBuilder
                                .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                                .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                                .OfRule("Rule Foo")
                                .WithPriority(IssuePriority.Warning)
                                .Create()
                        }));

                // When
                fixture.RunOrchestratorForIssueProviders();

                // Then
                fixture.PullRequestSystem.DiscussionThreadsCapability.ReopenedThreads.ShouldContain(threadToReopen);
                fixture.Log.Entries.ShouldContain(x => x.Message == "Found 1 existing thread(s) that are resolved but still have an open issue.");
            }

            [Fact]
            public void Should_Only_Reopen_Still_Active_Issues_From_Same_Comment_Source()
            {
                // Given
                var fixture =
                    new PullRequestsFixture(
                        (builder, settings) => builder
                            .WithDiscussionThreadsCapability(
                                new List<IPullRequestDiscussionThread>
                                {
                                    new PullRequestDiscussionThread(
                                        1,
                                        PullRequestDiscussionStatus.Resolved,
                                        new FilePath(@"src\Cake.Issues.Tests\FakeIssueProvider.cs"),
                                        new List<IPullRequestDiscussionComment>
                                        {
                                            new PullRequestDiscussionComment()
                                            {
                                                Content = "Message Foo",
                                                IsDeleted = false
                                            }
                                        })
                                    {
                                        CommentSource = "DifferentCommentSource",
                                        Resolution = PullRequestDiscussionResolution.Resolved
                                    }
                                }));

                var threadToReopen =
                    fixture.PullRequestSystem.DiscussionThreadsCapability.DiscussionThreads.Single();

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            IssueBuilder
                                .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                                .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                                .OfRule("Rule Foo")
                                .WithPriority(IssuePriority.Warning)
                                .Create()
                        }));

                // When
                fixture.RunOrchestratorForIssueProviders();

                // Then
                fixture.PullRequestSystem.DiscussionThreadsCapability.ReopenedThreads.ShouldBeEmpty();
            }

            [Fact]
            public void Should_Not_Throw_If_No_Existing_Threads()
            {
                // Given
                var fixture =
                    new PullRequestsFixture(
                        (builder, settings) => builder
                            .WithDiscussionThreadsCapability(
                                new List<IPullRequestDiscussionThread>()));

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            IssueBuilder
                                .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                                .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                                .OfRule("Rule Foo")
                                .WithPriority(IssuePriority.Warning)
                                .Create()
                        }));

                // When
                fixture.RunOrchestratorForIssueProviders();

                // Then
                fixture.Log.Entries.ShouldContain(x => x.Message == "No existings threads to resolve.");
                fixture.Log.Entries.ShouldContain(x => x.Message == "No existings threads to reopen.");
            }

            [Fact]
            public void Should_Output_A_Warning_If_Unknown_Thread_Status()
            {
                // Given
                var fixture =
                    new PullRequestsFixture(
                        (builder, settings) => builder
                            .WithDiscussionThreadsCapability(
                                new List<IPullRequestDiscussionThread>
                                {
                                    new PullRequestDiscussionThread(
                                        1,
                                        PullRequestDiscussionStatus.Unknown,
                                        new FilePath(@"src\Cake.Issues.Tests\FakeIssueProvider.cs"),
                                        new List<IPullRequestDiscussionComment>
                                        {
                                            new PullRequestDiscussionComment()
                                            {
                                                Content = "Message Foo",
                                                IsDeleted = false
                                            }
                                        })
                                }));

                var threadToReopen =
                    fixture.PullRequestSystem.DiscussionThreadsCapability.DiscussionThreads.Single();

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            IssueBuilder
                                .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                                .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                                .OfRule("Rule Foo")
                                .WithPriority(IssuePriority.Warning)
                                .Create()
                        }));

                // When
                fixture.RunOrchestratorForIssueProviders();

                // Then
                fixture.Log.Entries.ShouldContain(x => x.Message == "Thread has unknown status und matching comment(s) are ignored.");
            }
        }

        public sealed class TheRunMethodWithFilteringByModifiedFilesCapability
        {
            [Fact]
            public void Should_Ignore_Issues_If_File_Is_Not_Modified()
            {
                // Given
                var issue1 =
                    IssueBuilder
                        .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                        .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                        .OfRule("Rule Foo")
                        .WithPriority(IssuePriority.Warning)
                        .Create();
                var issue2 =
                    IssueBuilder
                        .NewIssue("Message Bar", "ProviderType Bar", "ProviderName Bar")
                        .InFile(@"src\Cake.Issues.Tests\NotModified.cs", 10)
                        .OfRule("Rule Bar")
                        .WithPriority(IssuePriority.Warning)
                        .Create();

                var fixture =
                    new PullRequestsFixture(
                        (builder, settings) => builder
                            .WithFilteringByModifiedFilesCapability(
                                new List<FilePath>
                                {
                                    new FilePath(@"src\Cake.Issues.Tests\FakeIssueProvider.cs")
                                }));

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            issue1,
                            issue2
                        }));

                // When
                var result = fixture.RunOrchestratorForIssueProviders();

                // Then
                result.ReportedIssues.Count().ShouldBe(2);
                result.ReportedIssues.ShouldContain(issue1);
                result.ReportedIssues.ShouldContain(issue2);
                result.PostedIssues.Count().ShouldBe(1);
                result.PostedIssues.ShouldContain(issue1);

                fixture.PullRequestSystem.PostedIssues.Count().ShouldBe(1);
                fixture.PullRequestSystem.PostedIssues.ShouldContain(issue1);

                fixture.Log.Entries.ShouldContain(x => x.Message == "1 issue(s) were filtered because they do not belong to files that were changed in this pull request");
                fixture.Log.Entries.ShouldContain(x => x.Message.StartsWith("Posting 1 issue(s):"));
            }

            [Fact]
            public void Should_Log_Message_If_All_Issues_Are_Filtered()
            {
                // Given
                var fixture =
                    new PullRequestsFixture(
                        (builder, settings) => builder
                            .WithFilteringByModifiedFilesCapability(
                                new List<FilePath>()));

                fixture.IssueProviders.Clear();
                fixture.IssueProviders.Add(
                    new FakeIssueProvider(
                        fixture.Log,
                        new List<IIssue>
                        {
                            IssueBuilder
                                .NewIssue("Message Foo", "ProviderType Foo", "ProviderName Foo")
                                .InFile(@"src\Cake.Issues.Tests\FakeIssueProvider.cs", 10)
                                .OfRule("Rule Foo")
                                .WithPriority(IssuePriority.Warning)
                                .Create(),
                            IssueBuilder
                                .NewIssue("Message Bar", "ProviderType Bar", "ProviderName Bar")
                                .InFile(@"src\Cake.Issues.Tests\NotModified.cs", 10)
                                .OfRule("Rule Bar")
                                .WithPriority(IssuePriority.Warning)
                                .Create()
                        }));

                // When
                fixture.RunOrchestratorForIssueProviders();

                // Then
                fixture.Log.Entries.ShouldContain(x => x.Message == "All issues were filtered. Nothing new to post.");
            }
        }
    }
}
