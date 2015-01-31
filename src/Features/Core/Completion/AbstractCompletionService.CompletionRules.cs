﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.Completion.Rules;
using Microsoft.CodeAnalysis.Shared.Utilities;

namespace Microsoft.CodeAnalysis.Completion
{
    internal partial class AbstractCompletionService
    {
        private class CompletionRules : ICompletionRules
        {
            private readonly AbstractCompletionService _completionService;
            private readonly PatternMatcher _patternMatcher = new PatternMatcher(verbatimIdentifierPrefixIsWordCharacter: true);

            public CompletionRules(AbstractCompletionService completionService)
            {
                _completionService = completionService;
            }

            public bool? MatchesFilterText(CompletionItem item, string filterText, CompletionTriggerInfo triggerInfo, CompletionFilterReason filterReason)
            {
                // If the user hasn't typed anything, and this item was preselected, or was in the
                // MRU list, then we definitely want to include it.
                if (filterText.Length == 0)
                {
                    if (item.Preselect || _completionService.GetMRUIndex(item) < 0)
                    {
                        return true;
                    }
                }

                if (IsAllDigits(filterText))
                {
                    // The user is just typing a number.  We never want this to match against
                    // anything we would put in a completion list.
                    return false;
                }

                var match = _patternMatcher.MatchPatternFirstOrNullable(
                            _completionService.GetCultureSpecificQuirks(item.FilterText),
                            _completionService.GetCultureSpecificQuirks(filterText));
                return match != null;
            }

            private bool IsAllDigits(string filterText)
            {
                for (int i = 0; i < filterText.Length; i++)
                {
                    if (filterText[i] < '0' || filterText[i] > '9')
                    {
                        return false;
                    }
                }

                return true;
            }

            public bool? IsBetterFilterMatch(CompletionItem item1, CompletionItem item2, string filterText, CompletionTriggerInfo triggerInfo, CompletionFilterReason filterReason)
            {
                var match1 = _patternMatcher.MatchPatternFirstOrNullable(
                            _completionService.GetCultureSpecificQuirks(item1.FilterText),
                            _completionService.GetCultureSpecificQuirks(filterText));
                var match2 = _patternMatcher.MatchPatternFirstOrNullable(
                            _completionService.GetCultureSpecificQuirks(item2.FilterText),
                            _completionService.GetCultureSpecificQuirks(filterText));

                if (match1 != null && match2 != null)
                {
                    var result = match1.Value.CompareTo(match2.Value);
                    if (result != 0)
                    {
                        return result < 0;
                    }
                }
                else if (match1 != null)
                {
                    return true;
                }
                else if (match2 != null)
                {
                    return false;
                }

                // If they both seemed just as good, but they differ on preselection, then
                // item1 is better if it is preselected, otherwise it it worse.
                if (item1.Preselect != item2.Preselect)
                {
                    return item1.Preselect;
                }

                // Prefer things with a keyword glyph, if the filter texts are the same.
                if (item1.Glyph != item2.Glyph && item1.FilterText == item2.FilterText)
                {
                    return item1.Glyph == Glyph.Keyword;
                }

                // They matched on everything, including preselection values.  Item1 is better if it
                // has a lower MRU index.

                var item1MRUIndex = _completionService.GetMRUIndex(item1);
                var item2MRUIndex = _completionService.GetMRUIndex(item2);

                // The one with the lower index is the better one.
                return item1MRUIndex < item2MRUIndex;
            }

            public bool? ShouldSoftSelectItem(CompletionItem item, string filterText, CompletionTriggerInfo triggerInfo)
            {
                return filterText.Length == 0 && !item.Preselect;
            }

            public void CompletionItemComitted(CompletionItem item)
            {
                _completionService.CompletionItemComitted(item);
            }

            public bool? ItemsMatch(CompletionItem item1, CompletionItem item2)
            {
                return
                    item1.FilterSpan == item2.FilterSpan &&
                    item1.SortText == item2.SortText;
            }
        }
    }
}
