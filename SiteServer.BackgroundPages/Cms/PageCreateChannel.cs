﻿using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using BaiRong.Core;
using SiteServer.BackgroundPages.Settings;
using SiteServer.CMS.Core;
using SiteServer.CMS.Core.Create;
using SiteServer.CMS.Model.Enumerations;

namespace SiteServer.BackgroundPages.Cms
{
    public class PageCreateChannel : BasePageCms
    {
        public ListBox LbNodeIdList;
        public DropDownList DdlScope;
        public Button BtnDeleteAll;

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            PageUtils.CheckRequestParameter("PublishmentSystemID");

            if (IsPostBack) return;

            VerifySitePermissions(AppManager.Permissions.WebSite.Create);

            var listitem = new ListItem("所有选中的栏目", "All");
            DdlScope.Items.Add(listitem);
            listitem = new ListItem("一个月内更新的栏目", "Month");
            DdlScope.Items.Add(listitem);
            listitem = new ListItem("一天内更新的栏目", "Day");
            DdlScope.Items.Add(listitem);
            listitem = new ListItem("2小时内更新的栏目", "2Hour");
            DdlScope.Items.Add(listitem);

            NodeManager.AddListItems(LbNodeIdList.Items, PublishmentSystemInfo, false, true, Body.AdminName);
            BtnDeleteAll.Attributes.Add("onclick", "return confirm(\"此操作将删除所有已生成的栏目页面，确定吗？\");");
        }

        public void Create_OnClick(object sender, EventArgs e)
        {
            if (!Page.IsPostBack || !Page.IsValid) return;

            var nodeIdList = new List<int>();
            var selectedNodeIdArrayList = ControlUtils.GetSelectedListControlValueArrayList(LbNodeIdList);

            var tableName = PublishmentSystemInfo.AuxiliaryTableForContent;

            if (DdlScope.SelectedValue == "Month")
            {
                var lastEditList = DataProvider.ContentDao.GetNodeIdListCheckedByLastEditDateHour(tableName, PublishmentSystemId, 720);
                foreach (var nodeId in lastEditList)
                {
                    if (selectedNodeIdArrayList.Contains(nodeId.ToString()))
                    {
                        nodeIdList.Add(nodeId);
                    }
                }
            }
            else if (DdlScope.SelectedValue == "Day")
            {
                var lastEditList = DataProvider.ContentDao.GetNodeIdListCheckedByLastEditDateHour(tableName, PublishmentSystemId, 24);
                foreach (var nodeId in lastEditList)
                {
                    if (selectedNodeIdArrayList.Contains(nodeId.ToString()))
                    {
                        nodeIdList.Add(nodeId);
                    }
                }
            }
            else if (DdlScope.SelectedValue == "2Hour")
            {
                var lastEditList = DataProvider.ContentDao.GetNodeIdListCheckedByLastEditDateHour(tableName, PublishmentSystemId, 2);
                foreach (var nodeId in lastEditList)
                {
                    if (selectedNodeIdArrayList.Contains(nodeId.ToString()))
                    {
                        nodeIdList.Add(nodeId);
                    }
                }
            }
            else
            {
                nodeIdList = TranslateUtils.StringCollectionToIntList(TranslateUtils.ObjectCollectionToString(selectedNodeIdArrayList));
            }

            if (nodeIdList.Count == 0)
            {
                FailMessage("请首先选中希望生成页面的栏目！");
                return;
            }

            foreach (var nodeId in nodeIdList)
            {
                CreateManager.CreateChannel(PublishmentSystemId, nodeId);
            }

            PageCreateStatus.Redirect(PublishmentSystemId);
        }

        public void BtnDeleteAll_OnClick(object sender, EventArgs e)
        {
            if (!Page.IsPostBack || !Page.IsValid) return;

            var url = PageProgressBar.GetDeleteAllPageUrl(PublishmentSystemId, ETemplateType.ChannelTemplate);
            PageUtils.RedirectToLoadingPage(url);
        }
    }
}
