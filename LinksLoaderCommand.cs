using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using LinksLoader.Models;
using LinksLoader.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinksLoader
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class LinksLoaderCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            LinkHelper linkHelper = new LinkHelper(doc);
            var windowViewModel = new WindowViewModel();
            var window = new LoaderView()
            {
                DataContext = windowViewModel
            };
            var worksetHelper = new WorksetHelper(doc);
            
            bool? result = window.ShowDialog();
            if (result == true)
            {
                List<string> selectedPaths = windowViewModel.treeViewModel.GetCheckedPaths();
                if (selectedPaths != null && selectedPaths.Count > 0)
                {
                    linkHelper.LoadLinks(selectedPaths);
                }
            }

            if (windowViewModel.DisableWorksets) {worksetHelper.TurnOffWorksets();}
            if (windowViewModel.MoveLinksToWorksets) { worksetHelper.CreateAndMoveWorksets(); }
            return Result.Succeeded;
        }
    }
}
