using System;
using System.Collections.Generic;
using System.IO;
using Chorus.merge;
using Chorus.Utilities;
using Chorus.Utilities.code;
using Chorus.VcsDrivers.Mercurial;
using Chorus.merge.xml.generic;
using Palaso.IO;
using Palaso.Progress;

namespace Chorus.FileTypeHanders
{
	/// <summary>
	/// Handler for files with extension of ".lift-ranges".
	/// </summary>
	public class LiftRangesFileTypeHandler : IChorusFileTypeHandler
	{
		internal LiftRangesFileTypeHandler()
		{}

		private const string Extension = "lift-ranges";

		public bool CanDiffFile(string pathToFile)
		{
			return CanValidateFile(pathToFile);
		}

		public bool CanMergeFile(string pathToFile)
		{
			return CanValidateFile(pathToFile);
		}

		public bool CanPresentFile(string pathToFile)
		{
			return CanValidateFile(pathToFile);
		}
		public bool CanValidateFile(string pathToFile)
		{
			if (string.IsNullOrEmpty(pathToFile))
				return false;
			if (!File.Exists(pathToFile))
				return false;
			var extension = Path.GetExtension(pathToFile);
			if (string.IsNullOrEmpty(extension))
				return false;
			if (extension[0] != '.')
				return false;

			return FileUtils.CheckValidPathname(pathToFile, Extension);
		}
		public string ValidateFile(string pathToFile, IProgress progress)
		{
			// TODO: Decide how we want to do validation. For now, just make sure it is well-formed xml.
			return XmlValidation.ValidateFile(pathToFile, progress);
		}

		public void Do3WayMerge(MergeOrder mergeOrder)
		{
			Guard.AgainstNull(mergeOrder, "mergeOrder");

			// <mergenotice>
			// When merging these changes into any .Net4 enabled branch, reject all of these changes.
			// Only the XmlMergeService.Do3WayMerge call shuoold be kept.
			// The .Net4 enabled branches handle the indent suppression in a different way and it is to be kept.
			XmlMergeService.CurrentSuppressIndentingChildren = XmlMergeService.LiftSuppressIndentingChildren;
			try
			{
				XmlMergeService.Do3WayMerge(mergeOrder,
					new LiftRangesMergingStrategy(mergeOrder.MergeSituation),
					false,
					null,
					"range", "id");
			}
			finally
			{
				XmlMergeService.CurrentSuppressIndentingChildren = XmlMergeService.DefaultSuppressIndentingChildren;
			}
			// </mergenotice>
		}

		public IEnumerable<IChangeReport> Find2WayDifferences(FileInRevision parent, FileInRevision child, HgRepository repository)
		{
			return Xml2WayDiffService.ReportDifferences(repository, parent, child, null, "range", "id");
		}

		public IChangePresenter GetChangePresenter(IChangeReport report, HgRepository repository)
		{
			return new DefaultChangePresenter(report, repository);
		}

		public IEnumerable<IChangeReport> DescribeInitialContents(FileInRevision fileInRevision, TempFile file)
		{
			return new IChangeReport[] { new DefaultChangeReport(fileInRevision, "Added") };
		}

		/// <summary>
		/// Get a list or one, or more, extensions this file type handler can process
		/// </summary>
		/// <returns>A collection of extensions (without leading period (.)) that can be processed.</returns>
		public IEnumerable<string> GetExtensionsOfKnownTextFileTypes()
		{
			yield return Extension;
		}

		/// <summary>
		/// Return the maximum file size that can be added to the repository.
		/// </summary>
		/// <remarks>
		/// Return UInt32.MaxValue for no limit.
		/// </remarks>
		public uint MaximumFileSize
		{
			get { return UInt32.MaxValue; }
		}
	}
}