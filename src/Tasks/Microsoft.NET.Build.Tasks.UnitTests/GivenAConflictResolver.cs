﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Build.Framework;
using Microsoft.NET.Build.Tasks.ConflictResolution;
using Microsoft.NET.Build.Tasks.UnitTests.Mocks;
using Xunit;

namespace Microsoft.NET.Build.Tasks.UnitTests
{
    public class GivenAConflictResolver
    {
        [Fact]
        public void ItemsWithDifferentKeysDontConflict()
        {
            var item1 = new MockConflictItem("System.Ben");
            var item2 = new MockConflictItem("System.Immo");

            var result = GetConflicts(item1, item2);

            result.Conflicts.Should().BeEmpty();
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        [Fact]
        public void WhenOnlyOneItemExistsAWinnerCannotBeDetermined()
        {
            var item1 = new MockConflictItem() { Exists = false };
            var item2 = new MockConflictItem() { Exists = true };

            var result = GetConflicts(item1, item2);

            result.Conflicts.Should().BeEmpty();
            result.UnresolvedConflicts.Should().Equal(item2);
        }

        [Fact]
        public void WhenNeitherItemExistsAWinnerCannotBeDetermined()
        {
            var item1 = new MockConflictItem() { Exists = false, AssemblyVersion = new Version("1.0.0.0") };
            var item2 = new MockConflictItem() { Exists = false, AssemblyVersion = new Version("2.0.0.0") };

            var result = GetConflicts(item1, item2);

            result.Conflicts.Should().BeEmpty();
            result.UnresolvedConflicts.Should().Equal(item2);
        }

        
        [Fact]
        public void WhenAnItemDoesntExistButDoesNotConflictWithAnythingItIsNotReported()
        {
            var result = GetConflicts(
                new MockConflictItem("System.Ben"),
                new MockConflictItem("System.Immo") { Exists = false },
                new MockConflictItem("System.Dave")
                );

            result.Conflicts.Should().BeEmpty();
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        [Fact]
        public void WhenItemsConflictAndDontHaveAssemblyVersionsTheFileVersionIsUsedToResolveTheConflict()
        {
            var item1 = new MockConflictItem() { AssemblyVersion = null, FileVersion = new Version("1.0.0.0") };
            var item2 = new MockConflictItem() { AssemblyVersion = null, FileVersion = new Version("3.0.0.0") };
            var item3 = new MockConflictItem() { AssemblyVersion = null, FileVersion = new Version("2.0.0.0") };

            var result = GetConflicts(item1, item2, item3);

            result.Conflicts.Should().Equal(item1, item3);
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        [Fact]
        public void WhenItemsConflictAndOnlyOneHasAnAssemblyVersionAWinnerCannotBeDetermined()
        {
            var item1 = new MockConflictItem() { AssemblyVersion = new Version("1.0.0.0") };
            var item2 = new MockConflictItem() { AssemblyVersion = null };

            var result = GetConflicts(item1, item2);

            result.Conflicts.Should().BeEmpty();
            result.UnresolvedConflicts.Should().Equal(item2);
        }

        [Fact]
        public void WhenItemsConflictAndAssemblyVersionsMatchTheFileVersionIsUsedToResolveTheConflict()
        {
            var item1 = new MockConflictItem() { FileVersion = new Version("3.0.0.0") };
            var item2 = new MockConflictItem() { FileVersion = new Version("2.0.0.0") };
            var item3 = new MockConflictItem() { FileVersion = new Version("1.0.0.0") };

            var result = GetConflicts(item1, item2, item3);

            result.Conflicts.Should().Equal(item2, item3);
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        [Fact]
        public void WhenItemsConflictTheAssemblyVersionIsUsedToResolveTheConflict()
        {
            var item1 = new MockConflictItem() { AssemblyVersion = new Version("1.0.0.0") };
            var item2 = new MockConflictItem() { AssemblyVersion = new Version("2.0.0.0") };
            var item3 = new MockConflictItem() { AssemblyVersion = new Version("3.0.0.0") };

            var result = GetConflicts(item1, item2, item3);

            result.Conflicts.Should().Equal(item1, item2);
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        [Fact]
        public void WhenItemsConflictAndDontHaveFileVersionsThePackageRankIsUsedToResolveTheConflict()
        {
            var item1 = new MockConflictItem() { FileVersion = null, PackageId = "Package3" };
            var item2 = new MockConflictItem() { FileVersion = null, PackageId = "Package2" };
            var item3 = new MockConflictItem() { FileVersion = null, PackageId = "Package1" };

            var result = GetConflicts(item1, item2, item3);

            result.Conflicts.Should().Equal(item1, item2);
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        [Fact]
        public void WhenItemsConflictAndOnlyOneHasAFileVersionAWinnerCannotBeDetermined()
        {
            var item1 = new MockConflictItem() { FileVersion = null };
            var item2 = new MockConflictItem() { FileVersion = new Version("1.0.0.0") };

            var result = GetConflicts(item1, item2);

            result.Conflicts.Should().BeEmpty();
            result.UnresolvedConflicts.Should().Equal(item2);
        }

        [Fact]
        public void WhenItemsConflictAndFileVersionsMatchThePackageRankIsUsedToResolveTheConflict()
        {
            var item1 = new MockConflictItem() { PackageId = "Package2" };
            var item2 = new MockConflictItem() { PackageId = "Package3" };
            var item3 = new MockConflictItem() { PackageId = "Package1" };

            var result = GetConflicts(item1, item2, item3);

            result.Conflicts.Should().Equal(item2, item1);
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        [Fact]
        public void WhenItemsConflictTheFileVersionIsUsedToResolveTheConflict()
        {
            var item1 = new MockConflictItem() { FileVersion = new Version("2.0.0.0") };
            var item2 = new MockConflictItem() { FileVersion = new Version("1.0.0.0") };
            var item3 = new MockConflictItem() { FileVersion = new Version("3.0.0.0") };

            var result = GetConflicts(item1, item2, item3);

            result.Conflicts.Should().Equal(item2, item1);
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        [Fact]
        public void WhenItemsConflictAndDontHaveAPackageRankTheItemTypeIsUsedToResolveTheConflict()
        {
            var item1 = new MockConflictItem() { PackageId = "Unranked1", ItemType = ConflictItemType.Platform };
            var item2 = new MockConflictItem() { PackageId = "Unranked2", ItemType = ConflictItemType.Reference };

            var result = GetConflicts(item1, item2);

            result.Conflicts.Should().Equal(item2);
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        [Fact]
        public void WhenItemsConflictAndOnlyOneHasAPackageRankItWins()
        {
            var item1 = new MockConflictItem() { PackageId = "Unranked1" };
            var item2 = new MockConflictItem() { PackageId = "Ranked1" };

            var result = GetConflicts(item1, item2);

            result.Conflicts.Should().Equal(item1);
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        [Fact]
        public void WhenItemsConflictAndPackageRanksMatchTheItemTypeIsUsedToResolveTheConflict()
        {
            var item1 = new MockConflictItem() { PackageId = "Package1", ItemType = ConflictItemType.Reference };
            var item2 = new MockConflictItem() { PackageId = "Package1", ItemType = ConflictItemType.Platform };

            var result = GetConflicts(item1, item2);

            result.Conflicts.Should().Equal(item1);
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        
        [Fact]
        public void WhenItemsConflictThePackageRankIsUsedToResolveTheConflict()
        {
            var item1 = new MockConflictItem() { PackageId = "Package1" };
            var item2 = new MockConflictItem() { PackageId = "Package2" };
            var item3 = new MockConflictItem() { PackageId = "Package3" };

            var result = GetConflicts(item1, item2, item3);

            result.Conflicts.Should().Equal(item2, item3);
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        [Fact]
        public void WhenItemsConflictAndBothArePlatformItemsTheConflictCannotBeResolved()
        {
            var item1 = new MockConflictItem() { ItemType = ConflictItemType.Platform };
            var item2 = new MockConflictItem() { ItemType = ConflictItemType.Platform };

            var result = GetConflicts(item1, item2);

            result.Conflicts.Should().BeEmpty();
            result.UnresolvedConflicts.Should().Equal(item2);
        }

        [Fact]
        public void WhenItemsConflictAndNeitherArePlatformItemsTheConflictCannotBeResolved()
        {
            var item1 = new MockConflictItem() { ItemType = ConflictItemType.Reference };
            var item2 = new MockConflictItem() { ItemType = ConflictItemType.CopyLocal };

            var result = GetConflicts(item1, item2);

            result.Conflicts.Should().BeEmpty();
            result.UnresolvedConflicts.Should().Equal(item2);
        }

        [Fact]
        public void WhenItemsConflictAPlatformItemWins()
        {
            var item1 = new MockConflictItem() { ItemType = ConflictItemType.Reference };
            var item2 = new MockConflictItem() { ItemType = ConflictItemType.Platform };

            var result = GetConflicts(item1, item2);

            result.Conflicts.Should().Equal(item1);
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        [Fact]
        public void WhenCommitWinnerIsFalseOnlyTheFirstResolvedConflictIsReported()
        {
            var committedItem = new MockConflictItem() { AssemblyVersion = new Version("2.0.0.0") } ;

            var uncommittedItem1 = new MockConflictItem() { AssemblyVersion = new Version("3.0.0.0") };
            var uncommittedItem2 = new MockConflictItem() { AssemblyVersion = new Version("1.0.0.0") };
            var uncommittedItem3 = new MockConflictItem() { AssemblyVersion = new Version("2.0.0.0") };

            var result = GetConflicts(new[] { committedItem }, uncommittedItem1, uncommittedItem2, uncommittedItem3);

            result.Conflicts.Should().Equal(committedItem);
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        [Fact]
        public void WhenCommitWinnerIsFalseAndThereIsNoWinnerEachUnresolvedConflictIsReported()
        {
            var committedItem = new MockConflictItem();

            var uncommittedItem1 = new MockConflictItem();
            var uncommittedItem2 = new MockConflictItem();
            var uncommittedItem3 = new MockConflictItem();

            var result = GetConflicts(new[] { committedItem }, uncommittedItem1, uncommittedItem2, uncommittedItem3);

            result.Conflicts.Should().BeEmpty();
            result.UnresolvedConflicts.Should().Equal(uncommittedItem1, uncommittedItem2, uncommittedItem3);
        }

        [Fact]
        public void WhenCommitWinnerIsFalseMultipleConflictsAreReportedIfTheCommittedItemWins()
        {
            var committedItem = new MockConflictItem() { AssemblyVersion = new Version("4.0.0.0") };

            var uncommittedItem1 = new MockConflictItem() { AssemblyVersion = new Version("3.0.0.0") };
            var uncommittedItem2 = new MockConflictItem() { AssemblyVersion = new Version("1.0.0.0") };
            var uncommittedItem3 = new MockConflictItem() { AssemblyVersion = new Version("2.0.0.0") };

            var result = GetConflicts(new[] { committedItem }, uncommittedItem1, uncommittedItem2, uncommittedItem3);

            result.Conflicts.Should().Equal(uncommittedItem1, uncommittedItem2, uncommittedItem3);
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        [Fact]
        public void WhenCommitWinnerIsFalseConflictsWithDifferentKeysAreReported()
        {
            var committedItem1 = new MockConflictItem("System.Ben") { AssemblyVersion = new Version("2.0.0.0") };
            var committedItem2 = new MockConflictItem("System.Immo") { AssemblyVersion = new Version("2.0.0.0") };

            var uncommittedItem1 = new MockConflictItem("System.Ben") { AssemblyVersion = new Version("1.0.0.0") };
            var uncommittedItem2 = new MockConflictItem("System.Immo") { AssemblyVersion = new Version("3.0.0.0") };
            var uncommittedItem3 = new MockConflictItem("System.Dave") { AssemblyVersion = new Version("3.0.0.0") };
            var uncommittedItem4 = new MockConflictItem("System.Ben") { AssemblyVersion = new Version("3.0.0.0") };

            var result = GetConflicts(new[] { committedItem1, committedItem2 }, uncommittedItem1, uncommittedItem2, uncommittedItem3, uncommittedItem4);

            result.Conflicts.Should().Equal(uncommittedItem1, committedItem2, committedItem1);
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        [Fact]
        public void WhenPackageOverridesAreSpecifiedTheyAreUsed()
        {
            var systemItem1 = new MockConflictItem("System.Ben") { PackageId = "System.Ben", PackageVersion = new Version("4.3.0") };
            var systemItem2 = new MockConflictItem("System.Immo") { PackageId = "System.Immo", PackageVersion = new Version("4.2.0") };
            var systemItem3 = new MockConflictItem("System.Dave") { PackageId = "System.Dave", PackageVersion = new Version("4.1.0") };

            var platformItem1 = new MockConflictItem("System.Ben") { PackageId = "Platform", PackageVersion = new Version("2.0.0") };
            var platformItem2 = new MockConflictItem("System.Immo") { PackageId = "Platform", PackageVersion = new Version("2.0.0") };
            var platformItem3 = new MockConflictItem("System.Dave") { PackageId = "Platform", PackageVersion = new Version("2.0.0") };

            var result = GetConflicts(
                new[] { systemItem1, systemItem2, systemItem3, platformItem1, platformItem2, platformItem3 },
                Array.Empty<MockConflictItem>(),
                new[] {
                    new MockTaskItem("Platform", new Dictionary<string, string>
                    {
                        { MetadataKeys.OverridenPackages, "System.Ben|4.3.0;System.Immo|4.3.0;System.Dave|4.3.0" },
                    })
                });

            result.Conflicts.Should().Equal(systemItem1, systemItem2, systemItem3);
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        [Fact]
        public void WhenAHigherPackageIsUsedPackageOverrideLoses()
        {
            var platformItem1 = new MockConflictItem("System.Ben") { PackageId = "Platform", PackageVersion = new Version("2.0.0") };
            var platformItem2 = new MockConflictItem("System.Immo") { PackageId = "Platform", PackageVersion = new Version("2.0.0") };
            var platformItem3 = new MockConflictItem("System.Dave") { PackageId = "Platform", PackageVersion = new Version("2.0.0") };

            var systemItem1 = new MockConflictItem("System.Ben") { PackageId = "System.Ben", PackageVersion = new Version("4.3.0") };
            var systemItem2 = new MockConflictItem("System.Immo") { PackageId = "System.Immo", PackageVersion = new Version("4.2.0") };
            // System.Dave has a higher PackageVersion than the PackageOverride
            var systemItem3 = new MockConflictItem("System.Dave")
            {
                PackageId = "System.Dave",
                PackageVersion = new Version("4.4.0"),
                AssemblyVersion = new Version(platformItem3.AssemblyVersion.Major + 1, 0)
            };

            var result = GetConflicts(
                new[] { systemItem1, systemItem2, systemItem3, platformItem1, platformItem2, platformItem3 },
                Array.Empty<MockConflictItem>(),
                new[] {
                    new MockTaskItem("Platform", new Dictionary<string, string>
                    {
                        { MetadataKeys.OverridenPackages, "System.Ben|4.3.0;System.Immo|4.3.0;System.Dave|4.3.0" },
                    })
                });

            result.Conflicts.Should().Equal(systemItem1, systemItem2, platformItem3);
            result.UnresolvedConflicts.Should().BeEmpty();
        }

        static ConflictResults GetConflicts(params MockConflictItem[] items)
        {
            return GetConflicts(items, Array.Empty<MockConflictItem>());
        }

        static ConflictResults GetConflicts(MockConflictItem[] itemsToCommit, params MockConflictItem[] itemsNotToCommit)
        {
            return GetConflicts(itemsToCommit, itemsNotToCommit, Array.Empty<ITaskItem>());
        }

        static ConflictResults GetConflicts(MockConflictItem[] itemsToCommit, MockConflictItem[] itemsNotToCommit, ITaskItem[] packageOverrides)
        {
            ConflictResults ret = new ConflictResults();

            void ConflictHandler(MockConflictItem item)
            {
                ret.Conflicts.Add(item);
            }

            void UnresolvedConflictHandler(MockConflictItem item)
            {
                ret.UnresolvedConflicts.Add(item);
            }

            string[] packagesForRank = itemsToCommit.Concat(itemsNotToCommit)
                .Select(i => i.PackageId)
                .Where(id => !id.StartsWith("Unranked", StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .OrderBy(id => id)
                .ToArray();

            var overrideResolver = new PackageOverrideResolver<MockConflictItem>(packageOverrides);

            var resolver = new ConflictResolver<MockConflictItem>(new PackageRank(packagesForRank), overrideResolver, new MockLog());

            resolver.ResolveConflicts(itemsToCommit, GetItemKey, ConflictHandler,
                unresolvedConflict: UnresolvedConflictHandler);

            resolver.ResolveConflicts(itemsNotToCommit, GetItemKey, ConflictHandler,
                commitWinner: false,
                unresolvedConflict: UnresolvedConflictHandler);

            return ret;
        }

        static string GetItemKey(MockConflictItem item)
        {
            return item.Key;
        }

        class ConflictResults
        {
            public List<MockConflictItem> Conflicts { get; set; } = new List<MockConflictItem>();
            public List<MockConflictItem> UnresolvedConflicts { get; set; } = new List<MockConflictItem>();
        }

        class MockLog : ILog
        {
            public void LogError(string message, params object[] messageArgs)
            {
            }

            public void LogMessage(string message, params object[] messageArgs)
            {
            }

            public void LogMessage(LogImportance importance, string message, params object[] messageArgs)
            {
            }

            public void LogWarning(string message, params object[] messageArgs)
            {
            }

            public void LogError(
            string subcategory,
            string errorCode,
            string helpKeyword,
            string file,
            int lineNumber,
            int columnNumber,
            int endLineNumber,
            int endColumnNumber,
            string message,
            params object[] messageArgs)
            {
            }

            public void LogWarning(
                string subcategory,
                string warningCode,
                string helpKeyword,
                string file,
                int lineNumber,
                int columnNumber,
                int endLineNumber,
                int endColumnNumber,
                string message,
                params object[] messageArgs)
            {
            }

            public void LogMessage(
                string subcategory,
                string code,
                string helpKeyword,
                string file,
                int lineNumber,
                int columnNumber,
                int endLineNumber,
                int endColumnNumber,
                MessageImportance importance,
                string message,
                params object[] messageArgs)
            {
            }
        }
    }
}
