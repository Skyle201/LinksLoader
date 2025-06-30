using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinksLoader.Models
{
    public class WorksetHelper
    {
        private readonly Document doc;

        public WorksetHelper(Document doc)
        {
            this.doc = doc;
        }
        public void TurnOffWorksets()
        {
            var linkInstances = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>();

            foreach (var linkInstance in linkInstances)
            {
                RevitLinkType linkType = doc.GetElement(linkInstance.GetTypeId()) as RevitLinkType;
                if (linkType == null) continue;

                ModelPath path = linkType.GetExternalFileReference()?.GetPath();
                if (path == null) continue;

                Document linkedDoc = linkInstance.GetLinkDocument();
                if (linkedDoc == null) continue;

                var worksets = new FilteredWorksetCollector(linkedDoc)
                    .OfKind(WorksetKind.UserWorkset)
                    .Cast<Workset>();

                var toOpen = worksets
                    .Where(ws =>
                    {
                        string name = ws.Name.ToLower();
                        return !(name.Contains("рмирован") || name.Contains("#") || name.Contains("*"));
                    })
                    .Select(ws => ws.Id)
                    .ToList();

                var config = new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets);
                config.Open(toOpen);

                try
                {
                    linkType.Unload(null);
                    linkType.LoadFrom(path, config);
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Ошибка", $"Не удалось перезагрузить связь {linkType.Name}: {ex.Message}");
                }
            }
        }


        public void CreateAndMoveWorksets()
        {
            List<string> requiredWorksetNames = new List<string>
    {
        "#DWG",
        "#Связи АР",
        "#Связи ВКиАУПТ",
        "#Связи КМ",
        "#Связи КР",
        "#Связи ОВ",
        "#Связи ТХ",
        "#Связи ЭОМиСС",
        "#Связи РФ"
    };

            WorksetTable worksetTable = doc.GetWorksetTable();
            ICollection<Workset> existingWorksets = new FilteredWorksetCollector(doc)
                .OfKind(WorksetKind.UserWorkset)
                .ToWorksets();

            Dictionary<string, Workset> worksetByName = existingWorksets
                .GroupBy(w => w.Name)
                .ToDictionary(g => g.Key, g => g.First());

            using (Transaction tx = new Transaction(doc, "Создание рабочих наборов и перемещение связей"))
            {
                tx.Start();

                foreach (string name in requiredWorksetNames)
                {
                    if (!worksetByName.ContainsKey(name))
                    {
                        Workset newWs = Workset.Create(doc, name);
                        worksetByName[name] = newWs;
                    }
                }

                var linkInstances = new FilteredElementCollector(doc)
                    .OfClass(typeof(RevitLinkInstance))
                    .Cast<RevitLinkInstance>();

                foreach (var linkInstance in linkInstances)
                {
                    string linkName = linkInstance.Name.ToLower();
                    string originalName = linkInstance.GetLinkDocument().Title;

                    Workset targetWorkset = GetTargetWorkset(linkName, worksetByName);
                    if (targetWorkset == null)
                    {
                        string dynamicName = $"#Связь {originalName}";
                        if (!worksetByName.ContainsKey(dynamicName))
                        {
                            Workset newDynamicWs = Workset.Create(doc, dynamicName);
                            worksetByName[dynamicName] = newDynamicWs;
                        }

                        targetWorkset = worksetByName[dynamicName];
                    }

                    Parameter param = linkInstance.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
                    if (param != null && !param.IsReadOnly)
                    {
                        param.Set(targetWorkset.Id.IntegerValue);
                    }

                    RevitLinkType linkType = doc.GetElement(linkInstance.GetTypeId()) as RevitLinkType;
                    if (linkType != null)
                    {
                        Parameter typeParam = linkType.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
                        if (typeParam != null && !typeParam.IsReadOnly)
                        {
                            typeParam.Set(targetWorkset.Id.IntegerValue);
                        }
                    }
                }

                tx.Commit();
            }
        }

        private Workset GetTargetWorkset(string name, Dictionary<string, Workset> worksetByName)
        {
            if (name.Contains("_бф") || name.Contains("_рф"))
                return worksetByName["#Связи РФ"];
            if (name.Contains("_ар") || name.Contains("_ar") || name.Contains("_аи"))
                return worksetByName["#Связи АР"];
            if (name.Contains("_кр") || name.Contains("_kr"))
                return worksetByName["#Связи КР"];
            if (name.Contains("_км") || name.Contains("_km"))
                return worksetByName["#Связи КМ"];
            if (name.Contains("_вк") || name.Contains("_vk") || name.Contains("_аупт"))
                return worksetByName["#Связи ВКиАУПТ"];
            if (name.Contains("_ов") || name.Contains("_ov") || name.Contains("_хс"))
                return worksetByName["#Связи ОВ"];
            if (name.Contains("_тх") || name.Contains("_th"))
                return worksetByName["#Связи ТХ"];
            if (name.Contains("_es") || name.Contains("_эс") || name.Contains("_сс"))
                return worksetByName["#Связи ЭОМиСС"];
            return null;
        }

    }
}
