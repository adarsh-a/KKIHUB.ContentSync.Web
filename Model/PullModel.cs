namespace KKIHUB.ContentSync.Web.Model
{
	using System;

	/// <summary>
	/// Defines pull model
	/// </summary>
	public class PullModel
	{
		/// <summary>
		/// Defines start date
		/// </summary>
		public DateTime StartDate { get; set; }

		/// <summary>
		/// Defines end date
		/// </summary>
		public DateTime EndDate { get; set; }

		/// <summary>
		/// Defines the source hub
		/// </summary>
		public string SourceHub { get; set; }

		/// <summary>
		/// Defines the target hub
		/// </summary>
		public string Targethub { get; set; }

		/// <summary>
		/// Defines sync id
		/// </summary>
		public string SyncId { get; set; }

		/// <summary>
		/// Defines library
		/// </summary>
		public string Library { get; set; }
	}
}