﻿using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace DocDb
{
	public class DocumentWrap<T> : Document
	{
		//[JsonProperty("Type")]
		//public string Type { get; set; }

		[JsonProperty("Document")]
		public T Document { get; set; }
	}

	public static class DocumentWrapHelper
	{
		public static DocumentWrap<T> Wrap<T>(this T obj, Func<T, string> selectId) where T : class
		{
			if (obj == null) return null;

			return new DocumentWrap<T>()
			{
				Id = DocumentWrapHelper.ConcateIds(obj.GetType().Name, selectId(obj)),
				Document = obj
				//Type = obj.GetType().Name
			};
		}



		public static string ConcateIds(params object[] list)
		{
			return list.Select(l => l.ToString()).Aggregate((current, next) => current + "|" + next);
        }
	}

}
