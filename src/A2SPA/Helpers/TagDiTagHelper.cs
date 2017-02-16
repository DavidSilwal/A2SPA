﻿using Humanizer;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace A2SPA.Helpers
{
    [HtmlTargetElement("tag-di")]
    public class TagDiTagHelper : TagHelper
    {
        /// <summary>
        /// Name of data property 
        /// </summary>
        [HtmlAttributeName("for")]
        public ModelExpression For { get; set; }


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var dataType = ((DefaultModelMetadata)For.Metadata).DataTypeName;

            var shortLabelName = ((DefaultModelMetadata)For.Metadata).DisplayName ?? For.Name.Humanize();
            var labelName = ((DefaultModelMetadata)For.Metadata).Placeholder ?? shortLabelName;
            var description = For.Metadata.Description ?? labelName;

            var labelTag = new TagBuilder("label");
            labelTag.InnerHtml.Append(description);
            labelTag.MergeAttribute("for", For.Name.Camelize());
            labelTag.AddCssClass("control-label");

            var inputTag = new TagBuilder("input");
            inputTag.AddCssClass("form-control");

            switch (dataType)
            {
                case "Password":
                    inputTag.MergeAttribute("type", dataType);
                    break;

                case "Currency":
                    inputTag.MergeAttribute("type", "number");
                    break;

                default:
                    inputTag.MergeAttribute("type", "text");
                    break;
            }

            inputTag.MergeAttribute("id", For.Name.Camelize());
            inputTag.MergeAttribute("name", For.Name.Camelize());
            inputTag.MergeAttribute("placeholder", shortLabelName);
            inputTag.MergeAttribute("#" + For.Name.Camelize(), "ngModel");

            TagBuilder validationBlock = new TagBuilder("div");
            validationBlock.MergeAttribute("*ngIf", string.Format("{0}.errors", For.Name.Camelize()));
            validationBlock.MergeAttribute("class", "alert alert-danger");

            if (((DefaultModelMetadata)For.Metadata).HasMinLengthValidation())
            {
                inputTag.Attributes.Add("minLength", ((DefaultModelMetadata)For.Metadata).MinLength().ToString());
            }

            if (((DefaultModelMetadata)For.Metadata).HasMaxLengthValidation())
            {
                inputTag.Attributes.Add("maxLength", ((DefaultModelMetadata)For.Metadata).MaxLength().ToString());
            }

            if (((DefaultModelMetadata)For.Metadata).IsRequired)
            {
                var requiredValidation = new TagBuilder("div");
                requiredValidation.MergeAttribute("[hidden]", string.Format("!{0}.errors.required", For.Name.Camelize()));
                requiredValidation.InnerHtml.Append(string.Format("{0} is required", labelName));
                validationBlock.InnerHtml.AppendHtml(requiredValidation);
                inputTag.Attributes.Add("required", "required");
            }

            inputTag.MergeAttribute("[(ngModel)]", For.CamelizedName());
            inputTag.TagRenderMode = TagRenderMode.StartTag;

            output.TagName = "div";
            output.Attributes.Add("class", "form-group");

            output.Content.AppendHtml(labelTag);

            // wrap the input tag with an input group, if needed
            switch (dataType)
            {
                case "Currency":
                    var divInputGroup = new TagBuilder("div");
                    divInputGroup.MergeAttribute("class", "input-group");
                    var spanInputGroupAddon = new TagBuilder("span");
                    spanInputGroupAddon.MergeAttribute("class", "input-group-addon");
                    spanInputGroupAddon.InnerHtml.Append("$");
                    divInputGroup.InnerHtml.AppendHtml(spanInputGroupAddon);
                    divInputGroup.InnerHtml.AppendHtml(inputTag);
                    output.Content.AppendHtml(divInputGroup);
                    break;

                default:
                    output.Content.AppendHtml(inputTag);
                    break;
            }

            output.Content.AppendHtml(validationBlock);
        }
    }
}