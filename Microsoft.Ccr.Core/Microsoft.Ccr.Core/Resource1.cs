using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.Ccr.Core
{
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode, CompilerGenerated]
    internal class Resource1
    {
        private static ResourceManager resourceMan;

        private static CultureInfo resourceCulture;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(Resource1.resourceMan, null))
                {
                    ResourceManager resourceManager = new ResourceManager("Microsoft.Ccr.Core.Resource1", typeof(Resource1).Assembly);
                    Resource1.resourceMan = resourceManager;
                }
                return Resource1.resourceMan;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get
            {
                return Resource1.resourceCulture;
            }
            set
            {
                Resource1.resourceCulture = value;
            }
        }

        internal static string ChoiceAlreadyActiveException
        {
            get
            {
                return Resource1.ResourceManager.GetString("ChoiceAlreadyActiveException", Resource1.resourceCulture);
            }
        }

        internal static string ChoiceBranchesCannotBePersisted
        {
            get
            {
                return Resource1.ResourceManager.GetString("ChoiceBranchesCannotBePersisted", Resource1.resourceCulture);
            }
        }

        internal static string DispatcherPortTestNotValidInThreadpoolMode
        {
            get
            {
                return Resource1.ResourceManager.GetString("DispatcherPortTestNotValidInThreadpoolMode", Resource1.resourceCulture);
            }
        }

        internal static string EnqueueTimerNotSupportedForClrThreadPoolDispatcherQueues
        {
            get
            {
                return Resource1.ResourceManager.GetString("EnqueueTimerNotSupportedForClrThreadPoolDispatcherQueues", Resource1.resourceCulture);
            }
        }

        internal static string ExceptionDuringArbiterCleanup
        {
            get
            {
                return Resource1.ResourceManager.GetString("ExceptionDuringArbiterCleanup", Resource1.resourceCulture);
            }
        }

        internal static string ExceptionDuringCausalityHandling
        {
            get
            {
                return Resource1.ResourceManager.GetString("ExceptionDuringCausalityHandling", Resource1.resourceCulture);
            }
        }

        internal static string HandleExceptionLog
        {
            get
            {
                return Resource1.ResourceManager.GetString("HandleExceptionLog", Resource1.resourceCulture);
            }
        }

        internal static string InterleaveCannotHaveFinalizerException
        {
            get
            {
                return Resource1.ResourceManager.GetString("InterleaveCannotHaveFinalizerException", Resource1.resourceCulture);
            }
        }

        internal static string InterleaveInvalidReceiverTaskArgumentForTryDequeuePendingItems
        {
            get
            {
                return Resource1.ResourceManager.GetString("InterleaveInvalidReceiverTaskArgumentForTryDequeuePendingItems", Resource1.resourceCulture);
            }
        }

        internal static string IteratorsCannotYieldToInterleaveException
        {
            get
            {
                return Resource1.ResourceManager.GetString("IteratorsCannotYieldToInterleaveException", Resource1.resourceCulture);
            }
        }

        internal static string IteratorsCannotYieldToReissueException
        {
            get
            {
                return Resource1.ResourceManager.GetString("IteratorsCannotYieldToReissueException", Resource1.resourceCulture);
            }
        }

        internal static string JoinReceiverDuplicatePortMessage
        {
            get
            {
                return Resource1.ResourceManager.GetString("JoinReceiverDuplicatePortMessage", Resource1.resourceCulture);
            }
        }

        internal static string JoinSinglePortReceiverAtLeastOneItemMessage
        {
            get
            {
                return Resource1.ResourceManager.GetString("JoinSinglePortReceiverAtLeastOneItemMessage", Resource1.resourceCulture);
            }
        }

        internal static string JoinsMustHaveOnePortMinimumException
        {
            get
            {
                return Resource1.ResourceManager.GetString("JoinsMustHaveOnePortMinimumException", Resource1.resourceCulture);
            }
        }

        internal static string ReceiveThunkInvalidOperation
        {
            get
            {
                return Resource1.ResourceManager.GetString("ReceiveThunkInvalidOperation", Resource1.resourceCulture);
            }
        }

        internal static string TaskAlreadyHasFinalizer
        {
            get
            {
                return Resource1.ResourceManager.GetString("TaskAlreadyHasFinalizer", Resource1.resourceCulture);
            }
        }

        internal static string TeardownBranchesCannotBePersisted
        {
            get
            {
                return Resource1.ResourceManager.GetString("TeardownBranchesCannotBePersisted", Resource1.resourceCulture);
            }
        }

        internal Resource1()
        {
        }
    }
}