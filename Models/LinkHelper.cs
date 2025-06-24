using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinksLoader.Models
{
    public class LinkHelper
    {
        Document doc;
        public LinkHelper(Document doc) { this.doc = doc; }
        public Result LoadLinks(List<string> strings)
        {
            try
            {
                foreach (string s in strings)
                {
                    var mp = ModelPathUtils.ConvertUserVisiblePathToModelPath(s);
                    var rlo = new RevitLinkOptions(false);
                    var placement_Shared = new ImportPlacement();
                    placement_Shared = ImportPlacement.Shared;
                    var placement_Origin = new ImportPlacement();
                    placement_Origin = ImportPlacement.Origin;
                    var sharedSucess = false;
                    using (Transaction tx = new Transaction(doc, "Загрузка связей"))
                    {
                        tx.Start();
                        try
                        {
                            var rl_type = RevitLinkType.Create(doc, mp, rlo);
                            var rl_inst = RevitLinkInstance.Create(doc, rl_type.ElementId, placement_Shared);
                            sharedSucess = true;
                            tx.Commit();
                        }
                        catch (Exception ex)
                        {
                            tx.RollBack();
                        }
                    }
                    if (!sharedSucess)
                    {
                        using (Transaction tx = new Transaction(doc, "Загрузка связей"))
                        {
                            tx.Start();
                            var rl_type = RevitLinkType.Create(doc, mp, rlo);
                            var rl_inst = RevitLinkInstance.Create(doc, rl_type.ElementId, placement_Origin);
                            tx.Commit();
                        }
                    }
                }
                return Result.Succeeded;
            }
            catch { return Result.Failed; } 
        }
    }
}