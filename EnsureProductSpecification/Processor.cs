using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using inRiver.Remoting;
using inRiver.Remoting.Objects;

namespace EnsureProductSpecification
{
    internal class Processor
    {
        private readonly Logger logger;

        private const string ParentEntity = "Product";
        private const string ParentName = "ProductName";
        private const string ChildEntity = "Item";
        private const string ParentChildLinkType = "ProductItem";
        private const string SpecificationLinkType = "ItemSpecifications";
        private const string SpecificationFieldType = "ProductSpecification";

        public Processor(Logger logger)
        {
            this.logger = logger;
            this.logger.Write(new string('=', 20));
        }

        public void Execute()
        {
            var parents = RemoteManager.DataService.GetAllEntitiesForEntityType(ParentEntity, LoadLevel.DataAndLinks);

            foreach (var parent in parents)
            {
                var data = parent.GetField(SpecificationFieldType).Data;

                if (string.IsNullOrEmpty(data?.ToString()))
                {
                    ProcessEmptySpecParent(parent);
                }
            }
        }

        private void ProcessEmptySpecParent(Entity parent)
        {
            var name = parent.GetField(ParentName).Data.ToString();

            var childlinks = RemoteManager.DataService.GetOutboundLinksForEntityAndLinkType(parent.Id, ParentChildLinkType);
            if (childlinks == null || !childlinks.Any())
            {
                logger.Write($"ERROR {ParentEntity} '{name}' does not have a specification and has no {ChildEntity}s to copy it from. Please fix manually.");
                return;
            }

            // get *all* the spec links of all children
            var specs = childlinks.Select(l => l.Target).SelectMany(child =>
            {
                var links = RemoteManager.DataService.GetOutboundLinksForEntityAndLinkType(child.Id,
                    SpecificationLinkType);
                return links.Select(l => Tuple.Create(l.Target.Id, l.Target.DisplayName.Data.ToString()));
            }).ToList();

            if (!specs.Any())
            {
                logger.Write($"ERROR {ParentEntity} '{name}' does not have a specification and neither do the {ChildEntity}s below it. Please fix manually.");
                return;
            }

            var groups = specs.GroupBy(s => s.Item1).ToList();

            if (groups.Count > 1)
            {
                var allspecs = string.Join(", ", groups.Select(g => $"'{g.First().Item2}' ({g.Count()})"));
                logger.Write($"ERROR {ParentEntity} '{name}' does not have a specification and it's children use multiple: {allspecs}. Please fix manually.");
                return;
            }

            // exactly one specification is used by all children (plus any number of unassigneds)
            var linktypeId = groups.Single().Key;

            parent.GetField(SpecificationFieldType).Data = linktypeId.ToString();
            RemoteManager.DataService.UpdateEntity(parent); // should trigger the "specification updated", to set *all* children to this value
            logger.Write($"INFO {ParentEntity} '{name}' did not have a specification and is updated to '{groups.Single().First().Item2}'.");
        }
    }
}
