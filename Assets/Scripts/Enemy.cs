using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private enum enemyMotion
    {
        None,
        Attack,
        Idle,
        Step,
        Dead,
    }
    enemyMotion curMotion = enemyMotion.None;
    enemyMotion beforeMotion = enemyMotion.None;


    [Header("스테이터스")]
    [SerializeReference] int monsterNumber;
    [SerializeField] private float curHp;
    [SerializeField] private float maxHp;
    [SerializeField] private float damage;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float respawnTime;

    [Header("액션")]
    [SerializeField] private float moveDelayCheck = 100.0f;
    [SerializeReference] private float moveSpeed;
    private bool boolMoveDelayCheck;//이동 후 딜레이체크 시작,종료 체크
    [SerializeField] private bool isMoving;//이동중인지 체크
    private bool isAttack;


    [Header("스프라이트 변경")]
    [SerializeField] private Sprite[] idle;//스프라이트 등록
    [SerializeField] private bool footCheck;//왼발 오른발 순서 체크
    private float spriteChangeDelay = 0.0f;
    [SerializeField] private float ratio = 0.0f;
    private int randomDirNumber;
    private bool checkChangeSpriteDelay;
    private Vector2 trsGaugeBarPos;
    Vector3 moveVec;
    Vector3 lookDir = Vector3.down;
    Vector3 target;
    Vector3 before;
    Vector3 after;
    private bool beforeSave;

    //[SerializeField] private Sprite sprHit;
    private Color sprDefault;
    SpriteRenderer sr;
    BoxCollider2D boxCollider2D;
    Rigidbody2D rigid;


    //private void OnValidate()
    //{
    //    if (gaugeBar != null)
    //        gaugeBar.SetHp(curHp, maxHp);
    //}
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isAttack == true && collision.gameObject.tag == "HitBox")
        {
            //Enemy enemySc = collision.GetComponent<Enemy>();
            //enemySc.DamagefromEnemy(Damage);
            //Destroy(gameObject);
        }
        if (collision.gameObject.tag == "Enemy")
        {
            Debug.Log("<color=aqua>destroy<color>");
            Destroy(transform.gameObject);
        }
    }

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        sprDefault = sr.color;
        boxCollider2D = GetComponent<BoxCollider2D>();
        SetStatus();
        curHp = maxHp;
    }
    private GaugeBar gaugeBar;
    GameObject HpGaugeBar;
    void Start()
    {
        monsterNumber = GameManager.Instance.GetMonsterNumber();//몬스터번호0번 토끼일때
        moveVec = transform.position;
        GameManager manager = GameManager.Instance;
        trsGaugeBarPos = transform.localPosition;
        GameObject HpGaugeBar = manager.GetGaugeBar();
        GameObject obj = Instantiate(HpGaugeBar, trsGaugeBarPos, Quaternion.identity, transform.transform);
        gaugeBar = obj.GetComponent<GaugeBar>();
        gaugeBar.SetHp(curHp, maxHp);
    }
    private void Update()
    {

    }
    void FixedUpdate()
    {
        getRandomNumber();
        //setMovingTarget();
        softMoving();
        checkNext();
        checkMoveDelay(moveSpeed);
        changeSprite();
        dead();
    }

    private void SetStatus()
    {
        if (monsterNumber == 0)
        {
            moveSpeed = UnityEngine.Random.Range(7.5f, 10.0f);
            maxHp = UnityEngine.Random.Range(19, 22);
        }
        if (monsterNumber == 1)
        {
            moveSpeed = UnityEngine.Random.Range(3.5f, 4.0f);
            maxHp = UnityEngine.Random.Range(90, 111);
        }
    }

    bool allStop;
    bool changeDice = true;
    private void getRandomNumber()
    {
        if (allStop == true) { return; }
        if (changeDice == false) { return; }
        if (changeDice == true)
        {
            //Debug.Log("getRandomNumber");
            randomDirNumber = UnityEngine.Random.Range(0, 5);
            switch (randomDirNumber)
            {
                case 0:
                    lookDir = Vector2.left;
                    sr.sprite = idle[0];
                    break;
                case 1:
                    lookDir = Vector2.right;
                    sr.sprite = idle[3];
                    break;
                case 2:
                    lookDir = Vector2.up;
                    sr.sprite = idle[6];
                    break;
                case 3:
                    lookDir = Vector2.down;
                    sr.sprite = idle[9];
                    break;
                case 4:
                    break;
            }
            changeDice = false;
        }
        RaycastHit2D[] hit = Physics2D.RaycastAll(boxCollider2D.bounds.center, lookDir, 0.5f);
        Debug.DrawRay(boxCollider2D.bounds.center, lookDir * 0.5f, Color.blue);
        setTarget = true;
        setMovingTarget();
    }

    bool setTarget;
    private void setMovingTarget()
    {
        if (allStop == true) { return; }
        if (setTarget == false) { return; }
        //Debug.Log("setMovingTarget");
        switch (randomDirNumber)
        {
            case 0: { target = new Vector3(transform.position.x - 0.5f, transform.position.y); break; }
            case 1: { target = new Vector3(transform.position.x + 0.5f, transform.position.y); break; }
            case 2: { target = new Vector3(transform.position.x, transform.position.y + 0.5f); break; }
            case 3: { target = new Vector3(transform.position.x, transform.position.y - 0.5f); break; }
            case 4: break;
        }
        before = new Vector3(transform.position.x, transform.position.y);
        beforeSave = true;
        isMoving = true;
        setTarget = false;
        softMoving();
    }

    private void softMoving()
    {
        if (allStop == true) { return; }
        if (isMoving == false) { return; }
        if (isMoving == true && beforeSave == true && boolMoveDelayCheck == false)
        {
            //Debug.Log("softMoving");
            ratio += Time.deltaTime * 1.8f;
            switch (randomDirNumber)
            {
                case 0:
                    {
                        after.x = Mathf.SmoothStep(before.x, target.x, ratio);
                        after.y = before.y;
                        break;
                    }
                case 1:
                    {
                        after.x = Mathf.SmoothStep(before.x, target.x, ratio);
                        after.y = before.y;
                        break;
                    }
                case 2:
                    {
                        after.y = Mathf.SmoothStep(before.y, target.y, ratio);
                        after.x = before.x;
                        break;
                    }
                case 3:
                    {
                        after.y = Mathf.SmoothStep(before.y, target.y, ratio);
                        after.x = before.x;
                        break;
                    }
            }
            transform.position = after;
        }
        if (isMoving == true && ratio >= 1.0f)
        {
            ratio = 0.0f;
            isMoving = false;
            beforeSave = false;
            curMotion = enemyMotion.Idle;
            checkChangeSpriteDelay = true;
            boolMoveDelayCheck = true;
            changeDice = true;
            nextAction = true;
            checkNext();
        }
    }

    bool nextAction;
    private void checkNext()
    {
        if (allStop == true) { return; }
        if (nextAction == false) { return; }
        //Debug.Log("checkNext");
        RaycastHit2D[] hit = Physics2D.RaycastAll(boxCollider2D.bounds.center, lookDir, 0.5f);
        Debug.DrawRay(boxCollider2D.bounds.center, lookDir * 0.5f, Color.cyan);
        if (monsterNumber == 0 && hit.Length != 1 && hit[1].transform.gameObject.tag == "Player")
        {
            allStop = true;
            run();
        }
        for (int iNum = 1; iNum < hit.Length; iNum++)
        {
            if (hit.Length != 1 && transform.position == hit[iNum].transform.position)
            {
                Debug.Log("<color=red>Destroy</color>");
                Destroy(transform.gameObject);
            }
        }
    }

    private void run()
    {
        before = new Vector3(transform.position.x, transform.position.y);
        if (lookDir == Vector3.left)
        {
            lookDir = Vector3.right;
            target = new Vector3(transform.position.x + 1.0f, transform.position.y);
            curMotion = enemyMotion.Step;
            checkChangeSpriteDelay = true;
        }
        if (lookDir == Vector3.right)
        {
            lookDir = Vector3.left;
            target = new Vector3(transform.position.x - 1.0f, transform.position.y);
            curMotion = enemyMotion.Step;
            checkChangeSpriteDelay = true;
        }
        if (lookDir == Vector3.up)
        {
            lookDir = Vector3.down;
            target = new Vector3(transform.position.x, transform.position.y - 1.0f);
            curMotion = enemyMotion.Step;
            checkChangeSpriteDelay = true;
        }
        if (lookDir == Vector3.down)
        {
            lookDir = Vector3.up;
            target = new Vector3(transform.position.x, transform.position.y + 1.0f);
            curMotion = enemyMotion.Step;
            checkChangeSpriteDelay = true;
        }
        isMoving = true;
        beforeSave = true;
        Debug.Log("run");
        curMotion = enemyMotion.Idle;
        allStop = false;
        softMoving();
        changeDice = true;
        //if체력일정이하 도망
    }

    private void checkMoveDelay(float _value)
    {
        if (allStop == true)
        {
            boolMoveDelayCheck = false;
            moveDelayCheck = 100.0f;
            return;
        }
        if (boolMoveDelayCheck == false) { return; }
        if (moveDelayCheck == 100.0f && boolMoveDelayCheck == true)
        {
            //Debug.Log("checkMoveDelay");
            moveDelayCheck -= _value;
            setTarget = false;
            isMoving = false;
        }
        if (moveDelayCheck != 100.0f)
        {
            moveDelayCheck += Time.deltaTime;
        }
        if (moveDelayCheck > 100 && boolMoveDelayCheck == true)
        {
            boolMoveDelayCheck = false;
            moveDelayCheck = 100.0f;
            changeDice = true;
        }
    }

    private void changeSprite()
    {
        if (checkChangeSpriteDelay == false) { return; }
        if (checkChangeSpriteDelay == true)
        {
            if (curMotion == enemyMotion.Step && footCheck == false)
            {
                if (spriteChangeDelay == 0)
                {
                    enemyMotionChange();
                    spriteChangeDelay = 0.3f;
                }
                if (spriteChangeDelay != 0)
                {
                    spriteChangeDelay -= Time.deltaTime;
                }
                if (spriteChangeDelay < 0)
                {
                    spriteChangeDelay = 0.0f;
                    curMotion = enemyMotion.Idle;
                    footCheck = true;
                }
            }
            if (curMotion == enemyMotion.Step && footCheck == true)
            {
                if (spriteChangeDelay == 0)
                {
                    enemyMotionChange();
                    spriteChangeDelay = 0.3f;
                }
                if (spriteChangeDelay != 0)
                {
                    spriteChangeDelay -= Time.deltaTime;
                }
                if (spriteChangeDelay < 0)
                {
                    spriteChangeDelay = 0.0f;
                    curMotion = enemyMotion.Idle;
                    footCheck = false;
                }
            }
            enemyMotionChange();
        }
    }
    private void enemyMotionChange()
    {
        switch (curMotion)
        {
            case enemyMotion.Idle:
                {
                    if (curMotion == enemyMotion.Idle && lookDir == Vector3.left && checkChangeSpriteDelay == true)
                    {
                        sr.sprite = idle[0];
                    }
                    if (curMotion == enemyMotion.Idle && lookDir == Vector3.right && checkChangeSpriteDelay == true)
                    {
                        sr.sprite = idle[3];
                    }
                    if (curMotion == enemyMotion.Idle && lookDir == Vector3.up && checkChangeSpriteDelay == true)
                    {
                        sr.sprite = idle[6];
                    }
                    if (curMotion == enemyMotion.Idle && lookDir == Vector3.down && checkChangeSpriteDelay == true)
                    {
                        sr.sprite = idle[9];
                    }
                    break;
                }
            case enemyMotion.Step:
                {
                    if (curMotion == enemyMotion.Step && lookDir == Vector3.left && footCheck == false && checkChangeSpriteDelay == true)
                    {
                        sr.sprite = idle[1];
                    }
                    if (curMotion == enemyMotion.Step && lookDir == Vector3.left && footCheck == true && checkChangeSpriteDelay == true)
                    {
                        sr.sprite = idle[2];
                    }
                    if (curMotion == enemyMotion.Step && lookDir == Vector3.right && footCheck == false && checkChangeSpriteDelay == true)
                    {
                        sr.sprite = idle[4];
                    }
                    if (curMotion == enemyMotion.Step && lookDir == Vector3.right && footCheck == true && checkChangeSpriteDelay == true)
                    {
                        sr.sprite = idle[5];
                    }
                    if (curMotion == enemyMotion.Step && lookDir == Vector3.up && footCheck == false && checkChangeSpriteDelay == true)
                    {
                        sr.sprite = idle[7];
                    }
                    if (curMotion == enemyMotion.Step && lookDir == Vector3.up && footCheck == true && checkChangeSpriteDelay == true)
                    {
                        sr.sprite = idle[8];
                    }
                    if (curMotion == enemyMotion.Step && lookDir == Vector3.down && footCheck == false && checkChangeSpriteDelay == true)
                    {
                        sr.sprite = idle[10];
                    }
                    if (curMotion == enemyMotion.Step && lookDir == Vector3.down && footCheck == true && checkChangeSpriteDelay == true)
                    {
                        sr.sprite = idle[11];
                    }
                    break;
                }
        }
        checkChangeSpriteDelay = false;
    }
    private void dead()
    {
        if (curHp > 0) { return; }
        else
        {
            Debug.Log("Dead");
            Destroy(gameObject);
        }
    }
    private void counterattack()
    {
        Debug.Log("counterattack");
    }
    public void DamagefromEnemy(float _damage)
    {
        Debug.Log($"Damage = {_damage}");
        curHp -= _damage;
        gaugeBar.SetHp(curHp, maxHp);
        Debug.Log($"CurHp = {curHp}");
        sprDefault = sr.color;
        sr.color = new Color(1, 1, 1, 0.4f);
        counterattack();
        Invoke("setSpriteDefault", 0.2f);
    }
    public float GetRespawnTime()
    {
        return respawnTime;
    }
    private void setSpriteDefault()
    {
        sr.color = sprDefault;
    }

    public void SetHp(GaugeBar _value)
    {
        gaugeBar = _value;
        gaugeBar.SetHp(curHp, maxHp);
    }
}
