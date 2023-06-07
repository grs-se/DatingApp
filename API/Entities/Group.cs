using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class Group
    {

        public Group()
        {

        }
        public Group(string name)
        {
            Name = name;
        }

        // Specify that Name is unique and is a primary key
        // because we do not use an Id field here
        [Key]
        public string Name { get; set; }
        public ICollection<Connection> Connections { get; set; } = new List<Connection>();
    }
}
