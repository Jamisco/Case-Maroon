using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CaseMaroon.GameSystem
{
    [CreateAssetMenu(fileName = "MessageManager", menuName = "CaseMaroon/Message Manager")]
    public class MessageManager : ScriptableObject
    {
        [SerializeField]
        private List<MessageData> Messages = new List<MessageData>();

        public MessageData GetMessage(string id)
        {
            return Messages.FirstOrDefault(x => x.id.Equals(id));
        }

        public bool TryAddMessage(MessageData data)
        {
            bool contains = Messages.Any(x => x.id.Equals(data.id));

            if(!contains)
            {
                Messages.Add(data);
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
