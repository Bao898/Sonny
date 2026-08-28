// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using Autodesk.Revit.UI;
using Sonny.Application.Domain.Services;

namespace SonnyBIM;

public class AutoJoinServices
{
    private readonly Document _doc;
    private readonly UIDocument _uiDoc;
    private readonly IProgressReporter _reporter;

    private readonly AutoJoinViewModel _viewModel;
    private TransactionGroup _transG;


    public AutoJoinServices(AutoJoinViewModel viewModel, IProgressReporter reporter)
    {
        _viewModel = viewModel;
        _doc = viewModel.Doc;
        _uiDoc = viewModel.UiDoc;
        _reporter = reporter;
        _transG = new TransactionGroup(_viewModel.Doc);

    }

}
