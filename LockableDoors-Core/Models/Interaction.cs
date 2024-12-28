using System;
using System.Collections.Generic;

namespace InteractableInteractionsAPI.Models
{
    public class Interaction
    {
        public Action Action;
        public Func<string> GetName { get; private set; }
        public Func<bool> GetDisabled { get; private set; }
        public ActionsTypesClass ActionsTypesClass
        {
            get
            {
                return new ActionsTypesClass
                {
                    Action = Action,
                    Name = GetName(),
                    Disabled = GetDisabled(),
                };
            }
        }

        /// <remarks>
        /// name can be a <see cref="string"/> or a <see cref="Func{}"/> that returns <see cref="string"/><br/>
        /// disabled can be a <see cref="bool"/> or a <see cref="Func{}"/> that returns <see cref="bool"/><br/>
        /// </remarks>
        public Interaction(string name, bool disabled, Action action) // simple static name and disabled
        {
            GetName = () => name;
            GetDisabled = () => disabled;
            Action = action;
        }

        public Interaction(Func<string> getName, Func<bool> getDisabled, Action action) // dynamic simple and static
        {
            GetName = getName;
            GetDisabled = getDisabled;
            Action = action;
        }

        public Interaction(string name, Func<bool> getDisabled, Action action) // simple name, dynamic disabled
        {
            GetName = () => name;
            GetDisabled = getDisabled;
            Action = action;
        }

        public Interaction(Func<string> getName, bool disabled, Action action) // dynamic name, simple disabled
        {
            GetName = getName;
            GetDisabled = () => disabled;
            Action = action;
        }

        public static List<ActionsTypesClass> GetActionsTypesClassList(List<Interaction> InteractionList)
        {
            List<ActionsTypesClass> actionsTypesClassList = new List<ActionsTypesClass>();

            foreach (Interaction interaction in InteractionList)
            {
                actionsTypesClassList.Add(interaction.ActionsTypesClass);
            }

            return actionsTypesClassList;
        }

        public static Interaction GetDisabledInteraction(string name)
        {
            return new Interaction(name, true, default(Action));
        }
    }

}
