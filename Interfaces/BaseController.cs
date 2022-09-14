using UnityEngine;

namespace Kwytto.Interfaces
{
    public class BaseController<U, C> : MonoBehaviour
        where U : BasicIUserMod<U, C>, new()
        where C : BaseController<U, C>
    {
        public void Start() => StartActions();

        protected virtual void StartActions() { }
    }
}