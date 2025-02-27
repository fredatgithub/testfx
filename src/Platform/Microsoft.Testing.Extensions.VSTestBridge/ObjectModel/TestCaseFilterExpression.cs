﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// NOTE: This file is copied as-is from VSTest source code.
using Microsoft.Testing.Platform;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Microsoft.Testing.Extensions.VSTestBridge.ObjectModel;

/// <summary>
/// Implements ITestCaseFilterExpression, providing test case filtering functionality.
/// </summary>
internal sealed class TestCaseFilterExpression : ITestCaseFilterExpression
{
    private readonly FilterExpressionWrapper _filterWrapper;

    /// <summary>
    /// If filter Expression is valid for performing TestCase matching
    /// (has only supported properties, syntax etc).
    /// </summary>
    private readonly bool _validForMatch;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestCaseFilterExpression"/> class.
    /// Adapter specific filter expression.
    /// </summary>
    public TestCaseFilterExpression(FilterExpressionWrapper filterWrapper)
    {
        Guard.NotNull(filterWrapper);
        _filterWrapper = filterWrapper;
        _validForMatch = RoslynString.IsNullOrEmpty(filterWrapper.ParseError);
    }

    /// <summary>
    /// Gets user specified filter criteria.
    /// </summary>
    public string TestCaseFilterValue => _filterWrapper.FilterString;

    /// <summary>
    /// Validate if underlying filter expression is valid for given set of supported properties.
    /// </summary>
    public string[]? ValidForProperties(IEnumerable<string>? supportedProperties, Func<string, TestProperty?> propertyProvider)
    {
        string[]? invalidProperties = null;
        if (_validForMatch)
        {
            invalidProperties = _filterWrapper.ValidForProperties(supportedProperties, propertyProvider);
        }

        return invalidProperties;
    }

    /// <summary>
    /// Match test case with filter criteria.
    /// </summary>
    public bool MatchTestCase(TestCase testCase, Func<string, object?> propertyValueProvider)
    {
        Guard.NotNull(testCase);
        Guard.NotNull(propertyValueProvider);

        return _validForMatch && _filterWrapper.Evaluate(propertyValueProvider);
    }
}
