using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAA_HowTo_ViewFilterByWorkset
{
    internal static class Utils
    {
        public static List<Workset> GetAllWorksets(Document curDoc)
        {
            List<Workset> worksetList = new List<Workset>();

            //get all user created worksets in current file
            WorksetKindFilter filter = new WorksetKindFilter(WorksetKind.UserWorkset);
            FilteredWorksetCollector worksetCollector = new FilteredWorksetCollector(curDoc);
            worksetCollector.WherePasses(filter);
            //worksetCollector.OfKind(WorksetKind.UserWorkset);
            //worksetCollector.OfKind(WorksetKind.OtherWorkset);

            foreach (Workset curWorkset in worksetCollector)
            {
                worksetList.Add(curWorkset);
            }

            return worksetList;
        }

        public static Workset GetWorksetByName(Document curDoc, string worksetName)
        {
            //get all user created worksets in current file
            List<Workset> worksetList = GetAllWorksets(curDoc);

            //loop through worksets and check if specified workset exists
            foreach (Workset curWorkset in worksetList)
            {
                if (curWorkset.Name == worksetName)
                {
                    return curWorkset;
                }
            }

            return null;
        }
        internal static RibbonPanel CreateRibbonPanel(UIControlledApplication app, string tabName, string panelName)
        {
            RibbonPanel currentPanel = GetRibbonPanelByName(app, tabName, panelName);

            if (currentPanel == null)
                currentPanel = app.CreateRibbonPanel(tabName, panelName);

            return currentPanel;
        }

        internal static RibbonPanel GetRibbonPanelByName(UIControlledApplication app, string tabName, string panelName)
        {
            foreach (RibbonPanel tmpPanel in app.GetRibbonPanels(tabName))
            {
                if (tmpPanel.Name == panelName)
                    return tmpPanel;
            }

            return null;
        }
    }
}
