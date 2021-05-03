﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using EPiServer.Cms.Shell;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Shell;
using Geta.Optimizely.ContentTypeIcons.Attributes;
using Geta.Optimizely.ContentTypeIcons.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Geta.Optimizely.ContentTypeIcons.Infrastructure.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(InitializableModule))]
    internal class TreeIconUiDescriptorInitialization : IInitializableModule
    {
        public static bool EnabledAndInUse { get; internal set; }
        public static bool FontAwesomeVersion4InUse { get; internal set; }
        public static bool FontAwesomeVersion5InUse { get; internal set; }

        public void Initialize(InitializationEngine context)
        {
            var registry = context.Locate.Advanced.GetInstance<UIDescriptorRegistry>();
            var configuration = context.Locate.Advanced.GetInstance<IOptions<ContentTypeIconOptions>>().Value;

            EnrichDescriptorsWithIconClass(registry.UIDescriptors, configuration);
        }

        internal void EnrichDescriptorsWithIconClass(
            IEnumerable<UIDescriptor> uiDescriptors,
            ContentTypeIconOptions configuration)
        {
            foreach (var descriptor in uiDescriptors)
            {
                EnrichDescriptorWithIconClass(descriptor, configuration);
            }
        }

        internal void EnrichDescriptorWithIconClass(UIDescriptor descriptor, ContentTypeIconOptions configuration)
        {
            var contentTypeIconAttribute = descriptor.ForType.GetCustomAttribute<ContentTypeIconAttribute>(false);
            var treeIconAttribute = descriptor.ForType.GetCustomAttribute<TreeIconAttribute>(false);

            if (contentTypeIconAttribute == null && treeIconAttribute?.Icon == null) return;

            if ((configuration.EnableTreeIcons && treeIconAttribute?.Ignore != true) || treeIconAttribute?.Icon != null)
            {
                descriptor.IconClass = BuildIconClassNames(contentTypeIconAttribute, treeIconAttribute);
                EnabledAndInUse = true;
            }
        }

        private static string BuildIconClassNames(ContentTypeIconAttribute contentTypeIconAttribute,
            TreeIconAttribute treeIconAttribute)
        {
            var icon = treeIconAttribute?.Icon ?? contentTypeIconAttribute?.Icon;
            if (icon == null) return string.Empty;

            var builder = new StringBuilder();
            var className = ToDashCase(icon.ToString()).Replace("_", string.Empty);

            switch (icon)
            {
                case FontAwesome _:
                    builder.AppendFormat("fa fa-{0} ", className);
                    FontAwesomeVersion4InUse = true;
                    break;
                case FontAwesome5Brands _:
                    builder.AppendFormat("fab fa-{0} ", className);
                    FontAwesomeVersion5InUse = true;
                    break;
                case FontAwesome5Regular _:
                    builder.AppendFormat("far fa-{0} ", className);
                    FontAwesomeVersion5InUse = true;
                    break;
                case FontAwesome5Solid _:
                    builder.AppendFormat("fas fa-{0} ", className);
                    FontAwesomeVersion5InUse = true;
                    break;
            }

            var rotate = treeIconAttribute?.Rotate ?? contentTypeIconAttribute.Rotate;

            switch (rotate)
            {
                case Rotations.Rotate90:
                case Rotations.Rotate180:
                case Rotations.Rotate270:
                    builder.AppendFormat("fa-rotate-{0} ", (int) rotate);
                    break;
                case Rotations.FlipHorizontal:
                    builder.Append("fa-flip-horizontal ");
                    break;
                case Rotations.FlipVertical:
                    builder.Append("fa-flip-vertical ");
                    break;
            }

            builder.Append("fa-fw");

            return builder.ToString();
        }

        private static string ToDashCase(string input)
        {
            return string.Concat(input.Select((c, i) =>
                i > 0 && char.IsUpper(c) && (!char.IsDigit(input[i - 1]) || !char.IsDigit(input[i - 2 > 0 ? i - 2 : 0]))
                    ? "-" + c
                    : c.ToString())).ToLower();
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}