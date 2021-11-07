namespace KKIHUB.ContentSync.Web.Model
{
	using System;

	/// <summary>
	/// Defines fetch content model
	/// </summary>
	public class FetchLibraryContentModel
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

		/// <summary>
		/// Defines recursivity status
		/// </summary>
		public bool Recursive { get; set; } = true;

		/// <summary>
		/// Defines if request is only for updated item
		/// </summary>
		public bool OnlyUpdated { get; set; } = true;


		#region constructor
		public FetchLibraryContentModel()
		{

		}

		public FetchLibraryContentModel(PullModel pullModel)
		{
			this.StartDate = pullModel.StartDate;

			this.EndDate = pullModel.EndDate;

			this.SyncId = pullModel.SyncId;

			this.Library = pullModel.Library;

			this.SourceHub = pullModel.SourceHub;
		}
		#endregion
	}
}