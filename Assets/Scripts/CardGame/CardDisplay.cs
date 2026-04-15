using UnityEngine;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public CardData cardData;
    public int cardIndex;

    //3D 카드 요소
    public MeshRenderer cardRenderer;
    public TextMeshPro nameText;
    public TextMeshPro costText;
    public TextMeshPro attackText;
    public TextMeshPro descriptionText;

    //카드 상태
    public bool isDragging = false;
    private Vector3 originalPosition;   //원본 위치

    //레이어 마스크
    public LayerMask enemyLayer;    //적레이어
    public LayerMask playerLayer;   //플레이어레이어

    public void Start()
    {
        playerLayer = LayerMask.GetMask("Player");
        enemyLayer = LayerMask.GetMask("Enemy");

        SetupCard(cardData);
    }

    public void SetupCard(CardData data)
    {
        cardData = data;

        if (nameText != null) nameText.text = data.cardName;
        if (costText != null) costText.text = data.manaCost.ToString();
        if (attackText != null) attackText.text = data.effectAmount.ToString();
        if (descriptionText != null) descriptionText.text = data.description;

        if(cardRenderer != null && data.artwork != null)
        {
            Material cardMaterial = cardRenderer.material;
            cardMaterial.mainTexture = data.artwork.texture;
        }

    }

    private void OnMouseDown()
    {
        //드래그 시작 시 원래 위치 저장
        originalPosition = transform.position;
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if(isDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        }
    }

    private void OnMouseUp()
    {
        if(CardManager.Instance.playerStats == null || CardManager.Instance.playerStats.currentMana < cardData.manaCost)
        {
            transform.position = originalPosition;
            Debug.Log($"마나가 부족합니다. (필요 : {cardData.manaCost} , 현재 : {CardManager.Instance.playerStats.currentMana}");
            return;
        }

        isDragging = false;

        //레이캐스트로 타겟 감지
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //카드 사용 판정
        bool cardUsed = false;

        //적 위에 드롭 했는지 검사
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer))
        {
            //적에게 공격 효과 적용
            CharacterStats enemyStats = hit.collider.GetComponent<CharacterStats>();

            if(enemyStats != null)
            {
                if(cardData.cardType == CardData.CardType.Attack)
                {
                    //공격 카드면 데미지 추가
                    enemyStats.TakeDamage(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName}로 {enemyStats.characterName}에게 {cardData.effectAmount} 데미지!");
                    cardUsed = true;
                }
                else
                {
                    Debug.Log("이 카드는 적에게 사용할 수 없습니다.");
                }
            }
        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer))
        {
            CharacterStats playerStats = hit.collider.GetComponent<CharacterStats>();

            if(playerStats != null)
            {
                if(cardData.cardType == CardData.CardType.Heal)
                {
                    //힐 카드면 체력 회복
                    playerStats.Heal(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName}로 {playerStats.characterName}의 체력을 {cardData.effectAmount} 회복!");
                    cardUsed = true;
                }
                else
                {
                    Debug.Log("이 카드는 플레이어에게 사용할 수 없습니다.");
                }
            }
        }
        else if(CardManager.Instance != null)
        {
            //버린 카드 더미 근처에 드롭 했는지 검새
            float distToDiscard = Vector3.Distance(transform.position, CardManager.Instance.discardPosition.position);
            if (distToDiscard < 2.0f)
            {
                //카드 버리기
                CardManager.Instance.DiscardCard(cardIndex);
                return;
            }
        }

        //카드를 사용하지 않으면 원래 위치로 되돌리기
        if (!cardUsed)
        {
            transform.position = originalPosition;
            CardManager.Instance.ArrangeHand();  //손패 재정렬
        }
        else
        {
            if(CardManager.Instance != null)
            {
                CardManager.Instance.DiscardCard(cardIndex); //카드 사용 후 버리기
            }

            //카드 사용 시 마나 소모
            CardManager.Instance.playerStats.UseMana(cardData.manaCost);
            Debug.Log($"마나를 {cardData.manaCost} 소모했습니다.");
        }
    }
}
