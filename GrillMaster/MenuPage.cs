using System;
using System.Collections;
using Microsoft.SPOT;

namespace GrillMaster
{
    public class MenuPage
    {
        private readonly Hashtable _pageButtonActions = new Hashtable();
        public readonly MenuState State;

        public MenuPage(MenuState state)
        {
            State = state;
        }

        public MenuPage AddBtn(Button btn, MenuState newState)
        {
            if (_pageButtonActions.Contains(btn))
                _pageButtonActions.Remove(btn);

            _pageButtonActions.Add(btn, newState);

            return this;
        }

        public MenuState GetNewState(Button btn)
        {
            if (!_pageButtonActions.Contains(btn))
                return MenuState.None;

            return (MenuState)_pageButtonActions[btn];
        }
    }
}
