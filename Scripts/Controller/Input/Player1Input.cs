using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
// using UnityEngine.InputSystem.Interactions;

// [CreateAssetMenu(menuName = "Player1 Input")]
public class Player1Input : ScriptableObject, Player1InputActions.IPlayActions
{
	Player1InputActions inputActions;

	// ! 注意每次更新按键表，都需在此处初始化接口
	public event UnityAction<Vector2> onMove = delegate {}; // 注意初始化为空委托，而不是null
	public event UnityAction onStopMove = delegate {};

	public event UnityAction onFire = delegate {};
	public event UnityAction onStopFire = delegate {};

	public event UnityAction onReload = delegate {};

	public event UnityAction onPreviousWeapon = delegate {};
	public event UnityAction onNextWeapon = delegate {};

	void OnEnable()
	{
		inputActions = new Player1InputActions();

		inputActions.Play.SetCallbacks(this);
		// ! 每个动作表 action map，都需在此处登记回调函数：inputActions.SheetName.SetCallbacks(this);
	}

	void OnDisable()
	{
		DisableAllInputs();
	}

	// 禁用所有输入
	public void DisableAllInputs()
	{
		inputActions.Play.Disable();
	}

	// 使用Play动作表时，可以隐藏并锁定鼠标
	public void EnablePlayInput()
	{
		inputActions.Play.Enable();

		// Cursor.visible = false;
		// Cursor.lockState = CursorLockMode.Locked;
	}

	// ! 注意每次更新按键表，都需在此处初始化接口

	public void OnMove(InputAction.CallbackContext context)
	{
		// context可接收信号。
		// 当玩家按下Move绑定的按键时，就会触发此处的onMove事件。
		if (context.performed)
			onMove.Invoke(context.ReadValue<Vector2>()); // onMove初始化不会为null

		// 当玩家松开Move绑定的按键时，就会触发此处的onStopMove事件。
		else if (context.canceled)
			onStopMove.Invoke();
	}

	public void OnFire(InputAction.CallbackContext context)
	{
		if (context.performed)
			onFire.Invoke();
		else if (context.canceled)
			onStopFire.Invoke();
	}

	public void OnReload(InputAction.CallbackContext context)
	{
		if (context.performed)
			onReload.Invoke();
	}

	public void OnPreviousWeapon(InputAction.CallbackContext context)
	{
		if (context.performed)
			onPreviousWeapon.Invoke();
	}

	public void OnNextWeapon(InputAction.CallbackContext context)
	{
		if (context.performed)
			onNextWeapon.Invoke();
	}
}
