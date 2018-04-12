﻿using Humanizer;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;

namespace A2SPA.Helpers
{
    /// <summary>
    /// Column Data Tag Helper - to create Table columns to data display
    /// </summary>
    [HtmlTargetElement("td", Attributes = columnDataAttribute)]
    public class TabCDTagHelper : TagHelper
    {
        private const string columnDataAttribute = "cdfor";

        /// <summary>
        /// Alternate name to set angular data-binding to
        /// </summary>
        [HtmlAttributeName("var")]
        public string Var { get; set; } = null;

        /// <summary>
        /// Alternate name to set angular parent data-binding to
        /// </summary>
        [HtmlAttributeName("par")]
        public string Par { get; set; } = null;

        /// <summary>
        /// Name of data property 
        /// </summary>
        [HtmlAttributeName(columnDataAttribute)]
        public ModelExpression CdFor { get; set; }

        /// <summary>
        /// Option: directly set display format using Angular 5 pipe and pipe format values
        /// </summary>
        ///<remarks>This attribute sets both pipe type and the pipe filter parameters.
        /// Numeric formats for decimal or percent in Angular use a string with the following format: 
        /// a.b-c where:
        ///     a = minIntegerDigits is the minimum number of integer digits to use.Defaults to 1.
        ///     b = minFractionDigits is the minimum number of digits after fraction.Defaults to 0.
        ///     c = maxFractionDigits is the maximum number of digits after fraction.Defaults to 3.
        /// </remarks>
        /// <example>
        /// to format a decimal value as a percentage use "|percent" for the default Angular
        /// or for a custom percentage value eg. "| percent:'1:3-5' 
        /// </example>
        [HtmlAttributeName("pipe")]
        public string Pipe { get; set; } = null;

        /// <summary>
        /// create angular pipe from server's local date/time format; currenly has support for short date and/or short time only.
        /// </summary>
        /// <example>
        /// local="short"
        /// </example>
        [HtmlAttributeName("local")]
        public string Local { get; set; } = null;

        /// <summary>
        /// use Moment.js to format the date, rather than Angular Datepipe.
        /// options: local, date, time, custom:YYYY-MM-DDTHH:mm
        /// </summary>
        /// <example>
        /// moment="local"
        /// </example>
        [HtmlAttributeName("moment")]
        public string Moment { get; set; } = null;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // get metadata names, property name and data type
            var metadata = ((DefaultModelMetadata)CdFor.Metadata);
            var propertyName = CdFor.Name.Camelize();
            var dataType = metadata.DataTypeName;
            var pipe = string.Empty;
            string tagContents = string.Empty;

            var localShortDateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToString().ToUpper() //.Replace("M","m").Replace("d","D")
                                                                                                                       // remove seconds
                                                    .Replace("TT", "A")
                                                    .Replace(":ss", string.Empty);
            // AM/PM needs a switch from tt to a
            // ensure leading zero
            localShortDateFormat = localShortDateFormat.Contains("DD") ? localShortDateFormat : localShortDateFormat.Replace("D", "DD");
            //// remove day name if present
            //localShortDateFormat = localShortDateFormat.Contains("ddd, ") ? localShortDateFormat : localShortDateFormat.Replace("ddd, ", string.Empty);
            var localShortTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToString().ToUpper()
                .Replace("TT", string.Empty); // "A"
            localShortTimeFormat = localShortTimeFormat.Contains("MM") ? localShortTimeFormat : localShortTimeFormat.Replace("M", "MM");
            var localformat = string.Empty;

            switch (dataType)
            {
                case "Date":
                    localformat = localShortDateFormat;
                    tagContents = "{{moment(" + CdFor.GetDataBindVariableName(Par, Var) + ").format('" + localformat + "')}}";
                    break;

                case "DateTime":
                    localformat = string.Format("{0} {1}", localShortDateFormat, localShortTimeFormat);
                    tagContents = "{{moment(" + CdFor.GetDataBindVariableName(Par, Var) + ").format('" + localformat + "')}}";
                    break;

                case "Time":
                    localformat = localShortTimeFormat;
                    tagContents = "{{moment(" + CdFor.GetDataBindVariableName(Par, Var) + ").format('" + localformat + "')}}";
                    break;

                default:
                    pipe = string.IsNullOrEmpty(Pipe) ? string.Empty : "|" + Pipe;
                    tagContents = CdFor.PopulateDataDisplayContents(pipe, Par, Var);
                    break;
            }

            output.Content.AppendHtml(tagContents);
        }
    }
}
