﻿using System;
using System.Collections.Specialized;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using SiteServer.BackgroundPages.Core;
using SiteServer.BackgroundPages.Utils;
using SiteServer.Utils;
using SiteServer.CMS.Core;
using SiteServer.CMS.Database.Core;
using SiteServer.CMS.Fx;
using SiteServer.Utils.Enumerations;

namespace SiteServer.BackgroundPages.Cms
{
    public class ModalUploadImage : BasePageCms
    {
        public HtmlInputFile HifUpload;

        public CheckBox CbIsTitleImage;
        public TextBox TbTitleImageWidth;
        public TextBox TbTitleImageHeight;

        public CheckBox CbIsShowImageInTextEditor;
        public CheckBox CbIsLinkToOriginal;
        public CheckBox CbIsSmallImage;
        public TextBox TbSmallImageWidth;
        public TextBox TbSmallImageHeight;

        public Literal LtlScript;

        private string _textBoxClientId;

        public static string GetOpenWindowString(int siteId, string textBoxClientId)
        {
            return LayerUtils.GetOpenScript("上传图片", FxUtils.GetCmsUrl(siteId, nameof(ModalUploadImage), new NameValueCollection
            {
                {"textBoxClientID", textBoxClientId}
            }), 600, 560);
        }

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            WebPageUtils.CheckRequestParameter("siteId");
            _textBoxClientId = AuthRequest.GetQueryString("TextBoxClientID");

            if (IsPostBack) return;

            ConfigSettings(true);

            CbIsTitleImage.Attributes.Add("onclick", "checkBoxChange();");
            CbIsShowImageInTextEditor.Attributes.Add("onclick", "checkBoxChange();");
            CbIsSmallImage.Attributes.Add("onclick", "checkBoxChange();");
        }

        private void ConfigSettings(bool isLoad)
        {
            if (isLoad)
            {
                if (!string.IsNullOrEmpty(SiteInfo.ConfigUploadImageIsTitleImage))
                {
                    CbIsTitleImage.Checked = TranslateUtils.ToBool(SiteInfo.ConfigUploadImageIsTitleImage);
                }
                if (!string.IsNullOrEmpty(SiteInfo.ConfigUploadImageTitleImageWidth))
                {
                    TbTitleImageWidth.Text = SiteInfo.ConfigUploadImageTitleImageWidth;
                }
                if (!string.IsNullOrEmpty(SiteInfo.ConfigUploadImageTitleImageHeight))
                {
                    TbTitleImageHeight.Text = SiteInfo.ConfigUploadImageTitleImageHeight;
                }

                if (!string.IsNullOrEmpty(SiteInfo.ConfigUploadImageIsShowImageInTextEditor))
                {
                    CbIsShowImageInTextEditor.Checked = TranslateUtils.ToBool(SiteInfo.ConfigUploadImageIsShowImageInTextEditor);
                }
                if (!string.IsNullOrEmpty(SiteInfo.ConfigUploadImageIsLinkToOriginal))
                {
                    CbIsLinkToOriginal.Checked = TranslateUtils.ToBool(SiteInfo.ConfigUploadImageIsLinkToOriginal);
                }
                if (!string.IsNullOrEmpty(SiteInfo.ConfigUploadImageIsSmallImage))
                {
                    CbIsSmallImage.Checked = TranslateUtils.ToBool(SiteInfo.ConfigUploadImageIsSmallImage);
                }
                if (!string.IsNullOrEmpty(SiteInfo.ConfigUploadImageSmallImageWidth))
                {
                    TbSmallImageWidth.Text = SiteInfo.ConfigUploadImageSmallImageWidth;
                }
                if (!string.IsNullOrEmpty(SiteInfo.ConfigUploadImageSmallImageHeight))
                {
                    TbSmallImageHeight.Text = SiteInfo.ConfigUploadImageSmallImageHeight;
                }
            }
            else
            {
                SiteInfo.ConfigUploadImageIsTitleImage = CbIsTitleImage.Checked.ToString();
                SiteInfo.ConfigUploadImageTitleImageWidth = TbTitleImageWidth.Text;
                SiteInfo.ConfigUploadImageTitleImageHeight = TbTitleImageHeight.Text;

                SiteInfo.ConfigUploadImageIsShowImageInTextEditor = CbIsShowImageInTextEditor.Checked.ToString();
                SiteInfo.ConfigUploadImageIsLinkToOriginal = CbIsLinkToOriginal.Checked.ToString();
                SiteInfo.ConfigUploadImageIsSmallImage = CbIsSmallImage.Checked.ToString();
                SiteInfo.ConfigUploadImageSmallImageWidth = TbSmallImageWidth.Text;
                SiteInfo.ConfigUploadImageSmallImageHeight = TbSmallImageHeight.Text;

                DataProvider.Site.Update(SiteInfo);
            }
        }

        public override void Submit_OnClick(object sender, EventArgs e)
        {
            if (CbIsTitleImage.Checked && string.IsNullOrEmpty(TbTitleImageWidth.Text) && string.IsNullOrEmpty(TbTitleImageHeight.Text))
            {
                FailMessage("缩略图尺寸不能为空！");
                return;
            }
            if (CbIsSmallImage.Checked && string.IsNullOrEmpty(TbSmallImageWidth.Text) && string.IsNullOrEmpty(TbSmallImageHeight.Text))
            {
                FailMessage("缩略图尺寸不能为空！");
                return;
            }

            ConfigSettings(false);

            if (HifUpload.PostedFile == null || "" == HifUpload.PostedFile.FileName) return;

            var filePath = HifUpload.PostedFile.FileName;
            try
            {
                var fileExtName = PathUtils.GetExtension(filePath).ToLower();
                var localDirectoryPath = PathUtility.GetUploadDirectoryPath(SiteInfo, fileExtName);
                var localFileName = PathUtility.GetUploadFileName(SiteInfo, filePath);
                var localTitleFileName = Constants.TitleImageAppendix + localFileName;
                var localSmallFileName = Constants.SmallImageAppendix + localFileName;
                var localFilePath = PathUtils.Combine(localDirectoryPath, localFileName);
                var localTitleFilePath = PathUtils.Combine(localDirectoryPath, localTitleFileName);
                var localSmallFilePath = PathUtils.Combine(localDirectoryPath, localSmallFileName);

                if (!PathUtility.IsImageExtenstionAllowed(SiteInfo, fileExtName))
                {
                    FailMessage("上传失败，上传图片格式不正确！");
                    return;
                }
                if (!PathUtility.IsImageSizeAllowed(SiteInfo, HifUpload.PostedFile.ContentLength))
                {
                    FailMessage("上传失败，上传图片超出规定文件大小！");
                    return;
                }

                HifUpload.PostedFile.SaveAs(localFilePath);

                var isImage = EFileSystemTypeUtils.IsImage(fileExtName);

                //处理上半部分
                if (isImage)
                {
                    FileUtility.AddWaterMark(SiteInfo, localFilePath);
                    if (CbIsTitleImage.Checked)
                    {
                        var width = TranslateUtils.ToInt(TbTitleImageWidth.Text);
                        var height = TranslateUtils.ToInt(TbTitleImageHeight.Text);
                        ImageUtils.MakeThumbnail(localFilePath, localTitleFilePath, width, height, true);
                    }
                }

                var imageUrl = PageUtility.GetSiteUrlByPhysicalPath(SiteInfo, localFilePath, true);
                if (CbIsTitleImage.Checked)
                {
                    imageUrl = PageUtility.GetSiteUrlByPhysicalPath(SiteInfo, localTitleFilePath, true);
                }

                var textBoxUrl = PageUtility.GetVirtualUrl(SiteInfo, imageUrl);

                var script = $@"
if (parent.document.getElementById('{_textBoxClientId}'))
{{
    parent.document.getElementById('{_textBoxClientId}').value = '{textBoxUrl}';
}}
";

                //处理下半部分
                if (CbIsShowImageInTextEditor.Checked && isImage)
                {
                    imageUrl = PageUtility.GetSiteUrlByPhysicalPath(SiteInfo, localFilePath, true);
                    var smallImageUrl = imageUrl;
                    if (CbIsSmallImage.Checked)
                    {
                        smallImageUrl = PageUtility.GetSiteUrlByPhysicalPath(SiteInfo, localSmallFilePath, true);
                    }

                    if (CbIsSmallImage.Checked)
                    {
                        var width = TranslateUtils.ToInt(TbSmallImageWidth.Text);
                        var height = TranslateUtils.ToInt(TbSmallImageHeight.Text);
                        ImageUtils.MakeThumbnail(localFilePath, localSmallFilePath, width, height, true);
                    }

                    var insertHtml = CbIsLinkToOriginal.Checked ? $@"<a href=""{imageUrl}"" target=""_blank""><img src=""{smallImageUrl}"" border=""0"" /></a>" : $@"<img src=""{smallImageUrl}"" border=""0"" />";

                    script += "if(parent." + UEditorUtils.GetEditorInstanceScript() + ") parent." + UEditorUtils.GetInsertHtmlScript("Content", insertHtml);
                }

                LtlScript.Text = $@"
<script type=""text/javascript"" language=""javascript"">
    {script}
    {LayerUtils.CloseScript}
</script>";
            }
            catch (Exception ex)
            {
                FailMessage(ex, ex.Message);
            }
        }
    }
}
