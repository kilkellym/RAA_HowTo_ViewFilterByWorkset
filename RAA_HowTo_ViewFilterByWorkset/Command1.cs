#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

#endregion

namespace RAA_HowTo_ViewFilterByWorkset
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // 1. get categories
            IList<ElementId> categories = new List<ElementId>();
            categories.Add(new ElementId(BuiltInCategory.OST_MechanicalEquipment));

            // 2. get workset Ids
            WorksetId HVACWSId = Utils.GetWorksetByName(doc, "HVAC").Id;
            WorksetId PlumbWSId = Utils.GetWorksetByName(doc, "Plumbing").Id;
            WorksetId MedGasWSId = Utils.GetWorksetByName(doc, "Medical Gas").Id;

            // 3. get parameter Ids
            ElementId worksetParam = new ElementId(BuiltInParameter.ELEM_PARTITION_PARAM);
            ElementId showInPlumbParam = GetProjectParameterId(doc, "SHOW IN PLUMBING");

            // 4. create individual rules
            FilterRule set1rule1 = ParameterFilterRuleFactory.CreateNotEqualsRule(worksetParam, new ElementId(HVACWSId.IntegerValue));
            FilterRule set1rule2 = ParameterFilterRuleFactory.CreateNotEqualsRule(worksetParam, new ElementId(MedGasWSId.IntegerValue));
            FilterRule set1rule3 = ParameterFilterRuleFactory.CreateNotEqualsRule(worksetParam, new ElementId(PlumbWSId.IntegerValue));

            FilterRule set2rule1 = ParameterFilterRuleFactory.CreateEqualsRule(worksetParam, new ElementId(PlumbWSId.IntegerValue));

            FilterRule set3rule1 = ParameterFilterRuleFactory.CreateEqualsRule(showInPlumbParam, 0);
            FilterRule set4rule1 = ParameterFilterRuleFactory.CreateHasNoValueParameterRule(showInPlumbParam);

            // 5. create lists of rules
            List<FilterRule> set1 = new List<FilterRule> { set1rule1, set1rule2, set1rule3 };
            List<FilterRule> set2 = new List<FilterRule> { set2rule1 };
            List<FilterRule> set3 = new List<FilterRule> { set3rule1 };
            List<FilterRule> set4 = new List<FilterRule> { set4rule1 };

            // 6. create element filters from lists of rules
            ElementFilter elemFilter1 = new ElementParameterFilter(set1);
            ElementFilter elemFilter2 = new ElementParameterFilter(set2);
            ElementFilter elemFilter3a = new ElementParameterFilter(set3);
            ElementFilter elemFilter3b = new ElementParameterFilter(set4);

            // 7. create hierachical groupings of OR and AND filters
            LogicalOrFilter Filter3 = new LogicalOrFilter(elemFilter3a, elemFilter3b);
            LogicalAndFilter Filter2 = new LogicalAndFilter(elemFilter2, Filter3 );
            LogicalOrFilter Filter1 = new LogicalOrFilter(new List<ElementFilter> { elemFilter1, Filter2 });

            // 8. create the view filter
            ParameterFilterElement filter = null;
            using(Transaction t = new Transaction(doc))
            {
                t.Start("Create filters");
                filter = ParameterFilterElement.Create(doc, "_Workset Test", categories, Filter1);
                
                t.Commit();
            }

            return Result.Succeeded;
        }
        internal ElementId GetProjectParameterId(Document doc, string name)
        {
            ParameterElement pElem = new FilteredElementCollector(doc)
                .OfClass(typeof(ParameterElement))
                .Cast<ParameterElement>()
                .Where(e => e.Name.Equals(name))
                .FirstOrDefault();

            return pElem?.Id;
        }
        public static ElementFilter CreateANDFilterFromFilterRules(IList<FilterRule> filterRules)
        {
            IList<ElementFilter> elemFilters = new List<ElementFilter>();
            foreach (FilterRule filterRule in filterRules)
            {
                ElementParameterFilter elemParamFilter = new ElementParameterFilter(filterRule);
                elemFilters.Add(elemParamFilter);
            }
            LogicalAndFilter elemFilter = new LogicalAndFilter(elemFilters);

            return elemFilter;
        }
        public static ElementFilter CreateORFilterFromFilterRules(IList<FilterRule> filterRules)
        {
            IList<ElementFilter> elemFilters = new List<ElementFilter>();
            foreach (FilterRule filterRule in filterRules)
            {
                ElementParameterFilter elemParamFilter = new ElementParameterFilter(filterRule);
                elemFilters.Add(elemParamFilter);
            }
            LogicalOrFilter elemFilter = new LogicalOrFilter(elemFilters);

            return elemFilter;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData1.Data;
        }
    }
}
