﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chorus.notes;
using Chorus.sync;
using Chorus.Utilities;

namespace Chorus.UI.Notes.Browser
{
	public class NotesInProjectViewModel
	{
		public delegate NotesInProjectViewModel Factory(IEnumerable<AnnotationRepository> repositories, IProgress progress);//autofac uses this

		private readonly IChorusUser _user;
		private readonly MessageSelectedEvent _messageSelectedEvent;
		private IEnumerable<AnnotationRepository> _repositories;
		private string _searchText;

		public NotesInProjectViewModel( IChorusUser user, IEnumerable<AnnotationRepository> repositories,
										MessageSelectedEvent messageSelectedEventToRaise, IProgress progress)
		{
			_user = user;
			_repositories = repositories;
			_messageSelectedEvent = messageSelectedEventToRaise;
//            foreach (var path in GetChorusNotesFilePaths(projectFolderConfiguration.FolderPath))
//            {
//                _repositories.Add(AnnotationRepository.FromFile(string.Empty, path, progress));
//            }
		}



		public bool ShowClosedNotes { get; set; }

		public IEnumerable<ListMessage> GetMessages()
		{
			foreach (var repository in _repositories)
			{
				IEnumerable<Annotation> annotations=  repository.GetAllAnnotations();
				if(ShowClosedNotes)
				{
					annotations= annotations.Where(a=>a.Status!="closed");
				}

				foreach (var annotation in annotations)
				{
					foreach (var message in annotation.Messages)
					{
						if (GetDoesMatch(annotation, message))
						{
							yield return new ListMessage(annotation, message);
						}
					}
				}
			}
		}

		private bool GetDoesMatch(Annotation annotation, Message message)
		{
			return string.IsNullOrEmpty(_searchText)
				   || annotation.LabelOfThingAnnotated.StartsWith(_searchText)
				   || annotation.ClassName.StartsWith(_searchText)
				   || message.Author.StartsWith(_searchText);
		}

		public void CloseAnnotation(ListMessage listMessage)
		{
			listMessage.ParentAnnotation.AddMessage(_user.Name, "closed", string.Empty);
		}

		public void SelectedMessageChanged(ListMessage listMessage)
		{
			if (_messageSelectedEvent != null)
			{
				if (listMessage == null) //nothing is selected now
				{
					_messageSelectedEvent.Raise(null, null);
				}
				else
				{
					_messageSelectedEvent.Raise(listMessage.ParentAnnotation, listMessage.Message);
				}
			}
		}

		public void SearchTextChanged(string searchText)
		{
			_searchText = searchText;
		}
	}
}