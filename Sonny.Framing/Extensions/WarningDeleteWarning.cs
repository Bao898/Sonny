// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

#region Namespaces

using System.Windows;
using Color = System.Drawing.Color;
using Line = Autodesk.Revit.DB.Line;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Input;
#endregion

namespace SonnyBIM
{
    /// <summary>
    /// Cái này sẽ tạo ra warning và cho phép chạy tiếp lệnh
    /// </summary>
    public class WarningDeleteWarning : IFailuresPreprocessor
    {
        public List<ElementId> FailingElementIds = new List<ElementId>();
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> failureMessages
                = failuresAccessor.GetFailureMessages();

            if (failureMessages.Any())
            {
                foreach (FailureMessageAccessor failure in failureMessages)
                {
                    if (failure.GetSeverity() == FailureSeverity.Error)
                        FailingElementIds.AddRange(failure.GetFailingElementIds().ToList());

                    //MessageBox.Show(failure.GetDescriptionText());
                    // MessageBox.Show(failure.GetSeverity().ToString());

                    FailureSeverity s = failure.GetSeverity();
                    if (s == FailureSeverity.Warning)
                    {
                        failuresAccessor.DeleteWarning(failure);
                        // MessageBox.Show("Test");
                    }
                    else if (s == FailureSeverity.Error)
                    {
                        failuresAccessor.ResolveFailure(failure);
                       // MessageBox.Show("Test");
                        //failuresAccessor.DeleteElements(failure.GetAdditionalElementIds().ToList());

                        //NumberErr += 1;
                    }
                    else if (s == FailureSeverity.DocumentCorruption)
                    {
                        return FailureProcessingResult.ProceedWithRollBack;
                    }
                }
                return FailureProcessingResult.ProceedWithCommit;
            }
            else
            {
                return FailureProcessingResult.Continue;
            }
        }
    }
}

