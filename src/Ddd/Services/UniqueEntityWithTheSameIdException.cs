using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddd.Services
{
	public class UniqueEntityWithTheSameIdException : Exception
	{
		public UniqueEntityWithTheSameIdException() : base("Unique entity with the same Id already exists!") { }
	}
}
