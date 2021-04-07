using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MenuManager
{
    public enum NodeStatus
    {
        Normal = 0,
        Disabled = 1,
        Hidden = 2
    }
    
    public struct Item
    {
        public byte Level;
        public string Name;
        public NodeStatus Status;
        public string NameMethod;
    }

    public class MenuHandler : IEnumerable
    {
        private readonly List<Item> _listItem = new List<Item>();

        public List<Item> ListItem => _listItem;

        public Item this[int index] {
            get => ListItem[index];
            set => ListItem[index] = value;
        }

        private void Read(Stream stream)
        {
            var streamReader = new StreamReader(stream);

            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                var item = new Item();
                var values = line.Split();
                if (!byte.TryParse(values[0], out item.Level) || !byte.TryParse(values[2], out var byteStatus))
                {
                    throw new ArgumentException("invalid stream");
                }
                item.Name = values[1];
                item.Status = (NodeStatus) byteStatus;
                item.NameMethod = values[3];
                ListItem.Add(item);
            }
        }

        public void Add(int index, Item item)
        {
            ListItem.Insert(index, item);
        }

        public void Load(Stream streamReader)
        {
            if (!streamReader.CanRead)
            {
                throw new ArgumentException("invalid stream");
            }

            ListItem.Clear();
            Read(streamReader);
        }

        public void Save(Stream streamWriter)
        {
            var stream = new StreamWriter(streamWriter);

            foreach (var item in ListItem)
            {
                stream.WriteLine($"{item.Level} {item.Name} {Convert.ToByte(item.Status)} {item.NameMethod}");
            }

            stream.Flush();
        }

        public void Delete(int index)
        {
            ListItem.RemoveAt(index);
        }

        public IEnumerator GetEnumerator()
        {
            return ListItem.GetEnumerator();
        }
    }
}