using UnityEngine;

namespace Kwytto.Interfaces
{
    public class BaseController<U, C> : BaseController
        where U : BasicIUserMod<U, C>, new()
        where C : BaseController<U, C>
    {
    }
    public class BaseController : MonoBehaviour
    {
        public void Start() => StartActions();

        protected virtual void StartActions() { }
    }
}