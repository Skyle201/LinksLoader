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
            LinkHelper helper = new LinkHelper(doc);

            var viewModel = new TreeViewModel();
            var window = new LoaderView()
            {
                DataContext = viewModel
            };

            bool? result = window.ShowDialog();
            if (result == true)
            {
                List<string> selectedPaths = viewModel.GetCheckedPaths();
                TaskDialog.Show("Список выбранных файлов", string.Join(Environment.NewLine, selectedPaths));
                if (selectedPaths != null && selectedPaths.Count > 0)
                {
                    helper.LoadLinks(selectedPaths);
                }
            }

            return Result.Succeeded;
        }
    }
}
