/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.EnterpriseArchitect.Manipulations;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.Manipulation;
using MDD4All.SpecIF.DataProvider.Contracts;
using System;
using System.Text.RegularExpressions;
using EAAPI = EA;

namespace MDD4All.SpecIF.DataIntegrator.EA.Extensions
{
    public static class EaDataIntegrationExtensions
    {
        public static void SetRequirementDataFromSpecIF(this EAAPI.Element eaRequirement,
                                                        Resource resource,
                                                        ISpecIfMetadataReader metadataReader)
        {
            string title = resource.GetPropertyValue("dcterms:title", metadataReader, "en");

            string description = resource.GetPropertyValue("dcterms:description", metadataReader, "en");

            string identifier = resource.GetPropertyValue("dcterms:identifier", metadataReader);

            eaRequirement.Name = title;

            eaRequirement.Notes = description;

            eaRequirement.Stereotype = "fmcreq";

            eaRequirement.Update();

            eaRequirement.SetTaggedValueString("specifId", resource.ID, false);

            eaRequirement.SetTaggedValueString("specifRevision", resource.Revision, false);

            eaRequirement.SetTaggedValueString("Identifier", identifier, false);

            string germanTitle = resource.GetPropertyValue("dcterms:title", metadataReader, "de");
            string germanDescription = resource.GetPropertyValue("dcterms:description", metadataReader, "de");

            eaRequirement.SetTaggedValueString("Second Title", germanTitle, false);
            eaRequirement.SetTaggedValueString("Second Description", HtmlToPlainText(germanDescription), true);

            string perspective = resource.GetPropertyValue("SpecIF:Perspective", metadataReader);

            if (!string.IsNullOrEmpty(perspective))
            {
                if (perspective == "V-perspective-1")
                {
                    eaRequirement.SetTaggedValueString("Perspective", "User");
                }
            }

            string statusId = resource.GetPropertyValue("SpecIF:LifeCycleStatus", metadataReader);

            string eaStatus = "";

            switch(statusId)
            {
                case "V-Status-0": // deprecated
                    eaStatus = "Deprecated";
                    break;

                case "V-Status-1": // rejected
                    eaStatus = "Rejected";
                    break;

                case "V-Status-2": // initial
                    eaStatus = "";
                    break;

                case "V-Status-3": // drafted
                    eaStatus = "Drafted";
                    break;

                case "V-Status-4": // submitted
                    eaStatus = "Submitted";
                    break;

                case "V-Status-5": // approved
                    eaStatus = "Approved";
                    break;

                case "V-Status-6": // done
                    eaStatus = "Completed";
                    break;

                case "V-Status-9": // validated
                    eaStatus = "Verified";
                    break;

                case "V-Status-7": // released
                    eaStatus = "Released";
                    break;

                default:
                    eaStatus = "";
                    break;


            }

            eaRequirement.Status = eaStatus;
            eaRequirement.Update();
        }

        public static void SetProjectDataFromSpecIF(this EAAPI.Package projectPackage,
                                                    ProjectDescriptor project)
        {
            EAAPI.Element packageElement = projectPackage.Element;

            packageElement.Name = project.GetTitleValue();

            packageElement.Stereotype = "specif project";

            packageElement.Update();
            projectPackage.Update();

            packageElement.SetTaggedValueString("specifProjectID", project.ID, false);


        }

        public static void SetHierarchyRootPackegeFromSpecIF(this EAAPI.Package hierarchyRootPackage,
                                                             Resource resource,
                                                             Node node,
                                                             ISpecIfMetadataReader metadataReader)
        {
            EAAPI.Element packageElement = hierarchyRootPackage.Element;

            string title = resource.GetPropertyValue("dcterms:title", metadataReader);

            hierarchyRootPackage.Name = title;
            hierarchyRootPackage.Update();

            packageElement.Stereotype = "specif hierarchy";

            packageElement.Update();


            packageElement.SetTaggedValueString("rootNodeID", node.ID, false);


        }

        private static string HtmlToPlainText(string html)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

            var text = html;
            //Decode html specific characters
            text = System.Net.WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);

            return text;
        }
    }
}
