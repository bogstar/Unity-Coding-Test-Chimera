using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoPanel : MonoBehaviour
{
    #region Editor parameters
    [SerializeField] private TextMeshProUGUI coordinatesLabel;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private GameObject unitDetails;
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private TextMeshProUGUI healthAmountLabel;
    [SerializeField] private RectTransform moveBar;
    [SerializeField] private TextMeshProUGUI moveAmountLabel;
    [SerializeField] private TextMeshProUGUI attackAmountLabel;
    [SerializeField] private TextMeshProUGUI descriptionLabel;
    [SerializeField] private TextMeshProUGUI allegienceLabel;
    [SerializeField] private TextMeshProUGUI attackRangeLabel;
    [SerializeField] private Image icon;
    #endregion

    #region Private fields
    private float healthBarTotalWidth;
    private float moveBarTotalWidth;
    #endregion

    private void Start()
    {
        healthBarTotalWidth = healthBar.sizeDelta.x;
        moveBarTotalWidth = moveBar.sizeDelta.x;
    }

    public void Display(bool display)
    {
        gameObject.SetActive(display);
    }

    public void SetInfo(Tile tile)
    {
        coordinatesLabel.text = tile.Position.x + ", " + tile.Position.y;

        if (tile.Unit == null)
        {
            nameLabel.text = "Empty";
            unitDetails.SetActive(false);
        }
        else
        {
            Unit unit = tile.Unit;

            nameLabel.text = unit.Name;
            unitDetails.SetActive(true);
            allegienceLabel.text = "Controlled by: <b>" + (unit.Allegiance == Allegiance.Player ? "<#00ff00>Player</color>" : "<#ff0000>Enemy</color>") + "</b>";
            healthAmountLabel.text = unit.CurrentHealth + " / " + unit.MaxHealth;
            moveAmountLabel.text = unit.MovementRemaining + " / " + unit.MovementRange;
            attackAmountLabel.text = unit.Attack.ToString();
            attackRangeLabel.text = unit.AttackRange.ToString();
            descriptionLabel.text = unit.Description;
            icon.sprite = unit.Icon;
            healthBar.sizeDelta = new Vector2(unit.CurrentHealth / (float)unit.MaxHealth * healthBarTotalWidth, healthBar.sizeDelta.y);
            moveBar.sizeDelta = new Vector2(unit.MovementRemaining / (float)unit.MovementRange * moveBarTotalWidth, moveBar.sizeDelta.y);
        }
    }
}
