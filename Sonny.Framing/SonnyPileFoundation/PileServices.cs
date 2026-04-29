// Licensed to the.NET Pile under one or more agreements.
// The.NET Pile licenses this file to you under the MIT license.

using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Sonny.Application.Domain.Services;
using Sonny.RevitExtensions.Extensions.Elements;

namespace SonnyBIM
{
    public class PileServices
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly PileFromCADViewModel _viewModel;
        private readonly IProgressReporter _reporter;
        private TransactionGroup _transG;
        private Transaction _trans;
        private List<ElementId> _newElementIds = new List<ElementId>();

        private List<Curve> _allLineToCreate = new List<Curve>();

        public PileServices(PileFromCADViewModel viewModel, IProgressReporter reporter)
        {
            _viewModel = viewModel;
            _doc = viewModel.Doc;
            _uiDoc = viewModel.UiDoc;
            _reporter = reporter;
            _transG = new TransactionGroup(_viewModel.Doc);
        }

        public void Execute()
        {
            _allLineToCreate = _viewModel.CadInstance.GetCurves();
            _allLineToCreate = _allLineToCreate.Where(x => x.CheckSameLayer(_viewModel.SelectedLayer, _viewModel.Doc))
                .ToList();
            List<InforPileModelFromCad> inforPileModelFromCads = new List<InforPileModelFromCad>();
            if (_viewModel.IsModelByTwoLine) {
                _allLineToCreate = _allLineToCreate.Where(x => x is Line).ToList();
                _allLineToCreate = _allLineToCreate.Distinct(new IEqualityComparerLineIntersect()).ToList();
                foreach (Curve curve in _allLineToCreate) {
                    Line? line = curve as Line;
                    inforPileModelFromCads.Add(new InforPileByTwoLineModelFromCad(line));
                }
            }
            else {
                List<Arc> arcs = _allLineToCreate.OfType<Arc>().ToList();
                IEnumerable<IGrouping<XyzExtension, Arc>> enumerable = arcs.GroupBy(x => new XyzExtension(x.Center));
                List<Arc?> arcs1 = enumerable.Select(x =>
                {
                    List<Arc> list = x.ToList();
                    Arc? firstOrDefault = list.FirstOrDefault(y => Math.Abs(y.Radius - list.Max(z => z.Radius)) < 1e-4);
                    return firstOrDefault;
                }).ToList();
                arcs1 = arcs1.Where(x => x != null).ToList();

                foreach (Arc arc in arcs1) {
                    inforPileModelFromCads.Add(new InforPileByCircleModelFromCad(arc));
                }
            }

            _trans = new Transaction(_doc);
            using (_trans) {
                int value = 0;
                _trans.Start("Run");
                FailureHandlingOptions failOpt = _trans.GetFailureHandlingOptions();
                failOpt.SetFailuresPreprocessor(new WarningDeleteWarning());
                _trans.SetFailureHandlingOptions(failOpt);

                foreach (InforPileModelFromCad inforPileModelFromCad in inforPileModelFromCads) {
                    if (_trans.HasStarted()) {
                        value++;
                        _reporter.Update(value, inforPileModelFromCads.Count);

                        try {
                            FamilySymbol familySymbol = null;
                            if (inforPileModelFromCad is InforPileByTwoLineModelFromCad inforPileByTwoLines) {
                                familySymbol = _viewModel.SelectedPile;
                            }
                            else {
                                InforPileByCircleModelFromCad byCircleModelFromCad = inforPileModelFromCad as InforPileByCircleModelFromCad;
                                familySymbol = FamilyUtils.GetFamilySymbolCircleColumn(_viewModel.SelectedFamily,
                                    byCircleModelFromCad.Diameter,
                                    _viewModel.SelectDiameterCirclePilePara);
                            }

                            if (familySymbol == null) {break; }

                            if (!familySymbol.IsActive) { familySymbol.Activate(); }

                            FamilyInstance instance = _doc.Create.NewFamilyInstance(inforPileModelFromCad.Center,familySymbol,
                                _viewModel.SelectedLevel,StructuralType.Footing);
                            instance.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set(0);
                            _newElementIds.Add(instance.Id);
                        }
                        catch (Exception) { }
                    }
                    else {
                        break;
                    }
                }

                if (_trans.HasStarted()) {
                    _reporter.Close();
                    _trans.Commit();

                    _newElementIds = _newElementIds.Where(id => id != null).ToList();
                    MessageBox.Show(string.Concat("You have created ",_newElementIds.Count," Pile Foundation!"),SonnyBIMConstraint.MessageBoxCaption,
                        MessageBoxButtons.OK,MessageBoxIcon.Information);
                    _uiDoc.Selection.SetElementIds(_newElementIds);
                }
            }
        }
    }
}















