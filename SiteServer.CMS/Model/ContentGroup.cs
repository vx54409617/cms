using Datory;

namespace SiteServer.CMS.Model
{
    [DataTable("siteserver_ContentGroup")]
    public class ContentGroup : Entity
    {
        [DataColumn]
        public string GroupName { get; set; }

        [DataColumn]
        public int SiteId { get; set; }

        [DataColumn]
        public int Taxis { get; set; }

        [DataColumn]
        public string Description { get; set; }
	}
}