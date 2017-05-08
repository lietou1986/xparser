using System;
using System.Collections.Generic;

namespace Dorado
{
    /// <summary>
    /// 用于记录操作的结果
    /// </summary>

    public class OperateResult : MarshalByRefObject
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="status">结果状态</param>
        /// <param name="description">结果描述信息</param>
        public OperateResult(OperateStatus status = OperateStatus.Success, string description = "操作成功")
        {
            Status = status;
            Description = description;
        }

        public OperateStatus Status { get; set; }

        public string Description { get; set; }

        #region Equals ...

        public override int GetHashCode()
        {
            return (int)Status;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(OperateResult))
                return false;

            return ((OperateResult)obj).Status == Status;
        }

        public static bool operator ==(OperateResult obj1, OperateResult obj2)
        {
            if (obj1 == null || obj2 == null)
                return obj1 == null && obj2 == null;

            return obj1.Status == obj2.Status;
        }

        public static bool operator !=(OperateResult obj1, OperateResult obj2)
        {
            return !(obj1 == obj2);
        }

        public static implicit operator bool(OperateResult op)
        {
            if (op == null)
                return false;

            return op.Status >= 0;
        }

        #endregion Equals ...

        /// <summary>
        /// 成功
        /// </summary>
        public static readonly OperateResult Success = new OperateResult(OperateStatus.Success, "操作成功！");

        /// <summary>
        /// 失败
        /// </summary>
        public static readonly OperateResult Failure = new OperateResult(OperateStatus.Failure, "操作失败！");
    }

    /// <summary>
    /// 带有附加数据的操作结果
    /// </summary>
    /// <typeparam name="TData"></typeparam>

    public class OperateResult<TData> : OperateResult
    {
        protected bool Equals(OperateResult<TData> other)
        {
            return base.Equals(other) && EqualityComparer<TData>.Default.Equals(Data, other.Data);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OperateResult<TData>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ EqualityComparer<TData>.Default.GetHashCode(Data);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="status">结果状态</param>
        /// <param name="description">结果描述信息</param>
        /// <param name="data">数据</param>
        public OperateResult(OperateStatus status = OperateStatus.Success, string description = "操作成功", TData data = default(TData))
            : base(status, description)
        {
            Data = data;
        }

        /// <summary>
        /// 附加数据
        /// </summary>

        public TData Data { get; set; }

        #region Equals ...

        public static bool operator ==(OperateResult<TData> obj1, OperateResult<TData> obj2)
        {
            if (obj1 == null || obj2 == null)
                return obj1 == null && obj2 == null;

            return obj1.Status == obj2.Status;
        }

        public static bool operator !=(OperateResult<TData> obj1, OperateResult<TData> obj2)
        {
            return !(obj1 == obj2);
        }

        public static implicit operator bool(OperateResult<TData> op)
        {
            if (op == null)
                return false;

            return op.Status >= 0;
        }

        #endregion Equals ...

        /// <summary>
        /// 成功
        /// </summary>
        public new static readonly OperateResult<TData> Success = new OperateResult<TData>(OperateStatus.Success, "操作成功！");

        /// <summary>
        /// 失败
        /// </summary>
        public new static readonly OperateResult<TData> Failure = new OperateResult<TData>(OperateStatus.Failure, "操作失败！");
    }
}