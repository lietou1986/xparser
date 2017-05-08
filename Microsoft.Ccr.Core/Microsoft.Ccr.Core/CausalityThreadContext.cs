using System;
using System.Collections.Generic;

namespace Microsoft.Ccr.Core
{
    internal class CausalityThreadContext
    {
        private ICollection<CausalityStack> Stacks;

        private ICausality ActiveCausality;

        internal Dictionary<Guid, ICausality> CausalityTable;

        public ICollection<ICausality> Causalities
        {
            get
            {
                if (ActiveCausality != null)
                {
                    return new ICausality[]
                    {
                        ActiveCausality
                    };
                }
                List<ICausality> list = new List<ICausality>();
                if (Stacks == null)
                {
                    return list;
                }
                foreach (CausalityStack current in Stacks)
                {
                    if (current.Count != 0)
                    {
                        list.Add(current[current.Count - 1]);
                    }
                }
                return list;
            }
        }

        public CausalityThreadContext(ICausality causality, ICollection<CausalityStack> stacks)
        {
            ActiveCausality = causality;
            Stacks = stacks;
        }

        public static bool IsEmpty(CausalityThreadContext context)
        {
            return context == null || (context.ActiveCausality == null && context.Stacks == null);
        }

        internal static bool RequiresDebugBreak(CausalityThreadContext context)
        {
            if (context == null)
            {
                return false;
            }
            if (context.ActiveCausality != null && context.ActiveCausality is Causality)
            {
                return ((Causality)context.ActiveCausality).BreakOnReceive;
            }
            if (context.Stacks != null)
            {
                foreach (CausalityStack current in context.Stacks)
                {
                    foreach (ICausality current2 in current)
                    {
                        Causality causality = current2 as Causality;
                        if (causality != null && causality.BreakOnReceive)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            return false;
        }

        private void Normalize()
        {
            if (ActiveCausality != null || Stacks == null)
            {
                return;
            }
            int num = 0;
            List<CausalityStack> list = null;
            foreach (CausalityStack current in Stacks)
            {
                if (current.Count == 0)
                {
                    if (list == null)
                    {
                        list = new List<CausalityStack>(1);
                    }
                    list.Add(current);
                }
                num += current.Count;
            }
            if (list == null && num > 1)
            {
                return;
            }
            if (list != null)
            {
                foreach (CausalityStack current2 in list)
                {
                    Stacks.Remove(current2);
                }
            }
            if (Stacks.Count == 0)
            {
                Stacks = null;
            }
            if (num == 1)
            {
                ActiveCausality = ((List<CausalityStack>)Stacks)[0][0];
                Stacks = null;
            }
        }

        internal void AddCausality(ICausality causality)
        {
            if (CausalityThreadContext.IsEmpty(this))
            {
                ActiveCausality = causality;
                return;
            }
            if (CausalityTable == null)
            {
                CausalityTable = new Dictionary<Guid, ICausality>();
            }
            if (ActiveCausality == null)
            {
                if (Stacks != null)
                {
                    if (CausalityTable.ContainsKey(causality.Guid))
                    {
                        return;
                    }
                    CausalityTable.Add(causality.Guid, causality);
                    foreach (CausalityStack current in Stacks)
                    {
                        current.Add(causality);
                    }
                }
                return;
            }
            if (causality.Guid == ActiveCausality.Guid)
            {
                return;
            }
            CausalityTable.Add(ActiveCausality.Guid, ActiveCausality);
            CausalityTable.Add(causality.Guid, causality);
            Stacks = new List<CausalityStack>();
            CausalityStack causalityStack = new CausalityStack();
            causalityStack.Add(ActiveCausality);
            causalityStack.Add(causality);
            ActiveCausality = null;
            Stacks.Add(causalityStack);
        }

        internal CausalityThreadContext Clone()
        {
            CausalityThreadContext causalityThreadContext = new CausalityThreadContext(ActiveCausality, null);
            if (ActiveCausality != null)
            {
                return causalityThreadContext;
            }
            causalityThreadContext.Stacks = new List<CausalityStack>();
            foreach (CausalityStack current in Stacks)
            {
                CausalityStack causalityStack = new CausalityStack();
                causalityStack.AddRange(current);
                causalityThreadContext.Stacks.Add(causalityStack);
            }
            return causalityThreadContext;
        }

        internal bool RemoveCausality(string name, ICausality causality)
        {
            if (ActiveCausality != null && ((causality != null && causality == ActiveCausality) || name == ActiveCausality.Name))
            {
                RemoveFromTable(name, causality);
                ActiveCausality = null;
                return true;
            }
            bool result = false;
            foreach (CausalityStack current in Stacks)
            {
                foreach (ICausality current2 in current)
                {
                    if ((causality != null && causality == current2) || name == current2.Name)
                    {
                        result = true;
                        current.Remove(current2);
                        RemoveFromTable(name, causality);
                        break;
                    }
                }
            }
            Normalize();
            return result;
        }

        private void RemoveFromTable(string name, ICausality causality)
        {
            if (CausalityTable != null)
            {
                if (causality != null)
                {
                    CausalityTable.Remove(causality.Guid);
                    return;
                }
                foreach (ICausality current in CausalityTable.Values)
                {
                    if (current.Name == name)
                    {
                        CausalityTable.Remove(current.Guid);
                        break;
                    }
                }
            }
        }

        internal void MergeWith(CausalityThreadContext context)
        {
            if (Stacks == null && ActiveCausality == null)
            {
                if (context.ActiveCausality != null)
                {
                    AddCausality(context.ActiveCausality);
                    return;
                }
                foreach (CausalityStack current in context.Stacks)
                {
                    AddCausalityStack(current);
                }
                return;
            }
            else
            {
                if (Stacks == null)
                {
                    Stacks = new List<CausalityStack>(1);
                }
                if (CausalityTable == null)
                {
                    CausalityTable = new Dictionary<Guid, ICausality>();
                }
                if (Stacks.Count == 0 && context.ActiveCausality != null)
                {
                    if (ActiveCausality.Guid == context.ActiveCausality.Guid)
                    {
                        return;
                    }
                    CausalityTable[ActiveCausality.Guid] = ActiveCausality;
                    CausalityTable[context.ActiveCausality.Guid] = context.ActiveCausality;
                    ((List<CausalityStack>)Stacks).Add(new CausalityStack());
                    ((List<CausalityStack>)Stacks)[0].Add(ActiveCausality);
                    ((List<CausalityStack>)Stacks).Add(new CausalityStack());
                    ((List<CausalityStack>)Stacks)[1].Add(context.ActiveCausality);
                    ActiveCausality = null;
                    return;
                }
                else
                {
                    if (ActiveCausality != null && context.Stacks != null)
                    {
                        ((List<CausalityStack>)Stacks).Add(new CausalityStack());
                        ((List<CausalityStack>)Stacks)[0].Add(ActiveCausality);
                        CausalityTable[ActiveCausality.Guid] = ActiveCausality;
                        foreach (CausalityStack current2 in context.Stacks)
                        {
                            AddCausalityStack(current2);
                        }
                        ActiveCausality = null;
                        Normalize();
                        return;
                    }
                    if (Stacks.Count > 0 && context.Stacks != null)
                    {
                        foreach (CausalityStack current3 in context.Stacks)
                        {
                            AddCausalityStack(current3);
                        }
                    }
                    return;
                }
            }
        }

        private void AddCausalityStack(CausalityStack s)
        {
            if (Stacks == null)
            {
                Stacks = new List<CausalityStack>(1);
            }
            CausalityStack causalityStack = new CausalityStack();
            if (s.Count > 0)
            {
                CausalityTable = new Dictionary<Guid, ICausality>();
                foreach (ICausality current in s)
                {
                    if (!CausalityTable.ContainsKey(current.Guid))
                    {
                        CausalityTable[current.Guid] = current;
                        causalityStack.Add(current);
                    }
                }
                if (causalityStack.Count > 0)
                {
                    Stacks.Add(causalityStack);
                }
            }
        }

        internal void PostException(Exception exception)
        {
            if (ActiveCausality != null)
            {
                if (ActiveCausality.ExceptionPort != null)
                {
                    ActiveCausality.ExceptionPort.TryPostUnknownType(exception);
                }
                ActiveCausality = null;
            }
            else
            {
                foreach (CausalityStack current in Stacks)
                {
                    ICausality causality = current[current.Count - 1];
                    current.RemoveAt(current.Count - 1);
                    if (causality.ExceptionPort != null)
                    {
                        causality.ExceptionPort.TryPostUnknownType(exception);
                    }
                }
            }
            Normalize();
        }
    }
}