using System.Collections.Generic;

namespace KKIHUB.ContentSync.Web.Model
{
    public class ContentModel
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string LibraryId { get; set; }
        public string LibraryName { get; set; }
        public string Filename { get; set; }
        public List<AssetModel> Assets { get; set; }

        public List<string> ReferencedItemIds { get; set; } = new List<string>();

        /// <summary>
        /// Defines the modified date
        /// </summary>
        public string ModifiedDate { get; set; }

        /// <summary>
        /// Defines the item name from target hub
        /// </summary>
        public string TargetHubItemName { get; set; }

        /// <summary>
        /// Defines modified date from target hub
        /// </summary>
        public string TargetHubModifiedDate { get; set; }
    }
}
