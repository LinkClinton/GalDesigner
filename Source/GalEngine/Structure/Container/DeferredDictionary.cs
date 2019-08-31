using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public class DeferredDictionary<TKey, TValue> : 
        ImmediateDictionary<TKey, TValue>
    {
        private struct Command
        {
            public TKey Key;
            public TValue Value;
            public CommandType CommandType;
        }

        public enum CommandType
        {
            Add,
            Remove,
            Clear
        }

        private List<Command> mCommands;

        public DeferredDictionary()
        {
            mCommands = new List<Command>();
        }

        public override void Add(TKey key, TValue value)
        {
            mCommands.Add(new Command()
            {
                Key = key,
                Value = value,
                CommandType = CommandType.Add
            });
        }

        public override void Remove(TKey key)
        {
            mCommands.Add(new Command()
            {
                Key = key,
                CommandType = CommandType.Remove
            });
        }

        public override void Clear()
        {
            mCommands.Add(new Command()
            {
                CommandType = CommandType.Clear
            });
        }

        public virtual void Flush()
        {
            mCommands.ForEach((command) =>
            {
                switch (command.CommandType)
                {
                    case CommandType.Add:
                        base.Add(command.Key, command.Value); break;
                    case CommandType.Remove:
                        base.Remove(command.Key); break;
                    case CommandType.Clear:
                        base.Clear(); break;
                    default: break;
                }
            });

            mCommands.Clear();
        }
    }
}
