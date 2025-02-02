using TMPro;
using UnityEngine;

public class SteeringBehaviors : MonoBehaviour
{

    // Posici�n
    // ya la tenemos a trav�s del transform.position

    // velocidad, cu�nto cambia la posici�n cada X tiempo. // Tiene direcci�n y magnitud
    protected Vector3 currentVelocity = Vector3.zero;

    // para limitar la velocidad de este agente en cada cuadro.
    [SerializeField]
    protected float maxVelocity = 10.0f;

    // Aceleraci�n, cu�nto cambia la velocidad cada X tiempo. // Tiene direcci�n y magnitud
    // protected Vector3 currentAcceleration = new Vector3();
    [SerializeField]
    protected float maxForce = 2.0f;


    // [SerializeField]
    // protected PredictableMovement ReferenciaEnemigo;
    protected GameObject ReferenciaObjetivo;
    protected Rigidbody targetRB;

    // si queda tiempo vemos c�mo quedar�a con esta forma de implementarlo.
    // protected PlayerControllerRef = null; 

    public void SetEnemyReference(GameObject enemyRef)
    { 
        ReferenciaObjetivo = enemyRef;
        // Debug.Log($"{name} tiene como objetivo a: {enemyRef.name}");

        // tenemos que checar si hay un rigidbody o no.
        if(ReferenciaObjetivo != null)
        { 
            targetRB = ReferenciaObjetivo.GetComponent<Rigidbody>();
            if(targetRB == null)
            {
                Debug.Log("El enemigo referenciado actualmente no tiene Rigidbody. �As� deber�a ser?");
            }
        }
    }

    //public void SetEnemyReference(PredictableMovement enemy)
    //{
    //    ReferenciaEnemigo = enemy;
    //}

    [SerializeField]
    protected Rigidbody rb;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    protected Vector3 Seek(Vector3 targetPosition)
    {
        Vector3 steeringForce = Vector3.zero;

        // La flecha que me lleva hacia mi objetivo lo m�s r�pido que yo podr�a ir.
        // el m�todo punta menos cola nos da la flecha hacia el objetivo.
        Vector3 desiredDirection = targetPosition - transform.position;

        Vector3 desiredDirectionNormalized = desiredDirection.normalized;

        //                      // la pura direcci�n hacia objetivo * mi m�xima velocidad posible
        Vector3 desiredVelocity = desiredDirectionNormalized        * maxVelocity;

        // Steering = velocidad deseada � velocidad actual
        steeringForce = desiredVelocity - rb.linearVelocity; // currentVelocity;

        return steeringForce;
    }

    protected Vector3 Flee(Vector3 targetPosition)
    {
        // Flee hace lo mismo que Seek pero en el sentido opuesto.
        // Lo hacemos del sentido opuesto usando el signo de menos '-'.
        return -Seek(targetPosition);
    }

    // Para pursuit necesitamos conocer la velocidad de nuestro objetivo.
    Vector3 Pursuit(Vector3 targetPosition, Vector3 targetCurrentVelocity)
    {
        // Cu�nta distancia hay entre mi objetivo y yo, dividida entre mi m�xima velocidad posible.
        float LookAheadTime = (transform.position - targetPosition).magnitude / maxVelocity;

        Vector3 predictedPosition = targetPosition + targetCurrentVelocity * LookAheadTime;

        return Seek(predictedPosition);
    }

    Vector3 Evade(Vector3 targetPosition, Vector3 targetCurrentVelocity)
    {
        // Cu�nta distancia hay entre mi objetivo y yo, dividida entre mi m�xima velocidad posible.
        float LookAheadTime = (transform.position - targetPosition).magnitude / maxVelocity;

        Vector3 predictedPosition = targetPosition + targetCurrentVelocity * LookAheadTime;

        return -Seek(predictedPosition);
    }

    private void FixedUpdate()
    {
        Vector3 steeringForce = Vector3.zero;

        // Vector3 steeringForce = Seek(ReferenciaEnemigo.transform.position);


        // Vector3 steeringForce = Pursuit(ReferenciaEnemigo.transform.position, ReferenciaEnemigo.rb.linearVelocity );

        // Solo aplicamos Pursuit si el objetivo que estamos persiguiendo tiene un Rigidbody.
        if (ReferenciaObjetivo != null)
        {
            if (targetRB != null)
            {
                steeringForce = Pursuit(ReferenciaObjetivo.transform.position, targetRB.linearVelocity);
            }
            else if (ReferenciaObjetivo != null)
            {
                steeringForce = Seek(ReferenciaObjetivo.transform.position);
            }
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }

        // Deber�a estar aqu� pero ahorita no hace nada, seg�n yo.
        steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);

        rb.AddForce(steeringForce, ForceMode.Acceleration);

        if(rb.linearVelocity.magnitude > maxVelocity)
            Debug.LogWarning(rb.linearVelocity);
    }

    private void OnDrawGizmos()
    {
        Vector3 targetPosition = Vector3.zero;
        Vector3 targetCurrentVelocity = Vector3.zero;

        if (ReferenciaObjetivo != null)
        {
            targetPosition = ReferenciaObjetivo.transform.position;
            if (targetRB != null)
            { targetCurrentVelocity = targetRB.linearVelocity; }
        }
            
        float LookAheadTime = (transform.position - targetPosition).magnitude / maxVelocity;


        Vector3 predictedPosition = targetPosition + targetCurrentVelocity * LookAheadTime;

        Gizmos.DrawCube(predictedPosition, Vector3.one);

        Gizmos.DrawLine(transform.position, predictedPosition);

        Gizmos.color = Color.red;
        // Hacemos una l�nea de la velocidad que tiene este agente ahorita.
        Gizmos.DrawLine (transform.position, transform.position + rb.linearVelocity.normalized * 3);

        // Dibujamos las fuerzas.
        Gizmos.color = Color.green;

        Vector3 steeringForce = Vector3.zero;

        if (targetRB != null && ReferenciaObjetivo != null)
            steeringForce = Pursuit(ReferenciaObjetivo.transform.position, targetCurrentVelocity);

        steeringForce = Vector3.ClampMagnitude(steeringForce, maxForce);

        Gizmos.DrawLine(transform.position, transform.position + steeringForce);

    }

    // Update is called once per frame
    /* void Update()
    {
        // Para saber hacia d�nde ir, aplicamos el m�todo punta menos cola, 
        // ReferenciaEnemigo va a ser la punta.
        // La posici�n del due�o de este script va a ser la Cola del vector.
        // Vector3 Difference = ReferenciaEnemigo.transform.position - transform.position;

        // le aplicamos la fuerza del seek a nuestro agente.
        // con esto, no siempre vamos a acelerar la m�xima cantidad.
        Vector3 accelerationVector = Seek(ReferenciaEnemigo.transform.position);

        // nuestra velocidad debe de incrementar de acuerdo a nuestra aceleraci�n.
        currentVelocity += accelerationVector * Time.deltaTime;


        // Queremos obtener velocidad hacia el objetivo.
        // currentVelocity += Difference;
        Debug.Log($"currentVelocity antes de limitarla {currentVelocity}");

        // c�mo limitan el valor de una variable?
        if (currentVelocity.magnitude < maxVelocity)
        {
            // entonces la velocidad se queda como est�, porque no es mayor que max velocity.
        }
        else
        {
            // si no, haces que la velocidad actual sea igual que la velocidad m�xima.
            currentVelocity = currentVelocity.normalized * maxVelocity;
            Debug.Log($"currentVelocity despu�s de limitarla {currentVelocity}");
        }

        //if(Difference.magnitude < DetectionDistance)
        //{
        //    // ya lo detectaste.
        //}



        // Ahora hacemos que la velocidad cambie nuestra posici�n conforme al paso del tiempo.
        transform.position += currentVelocity * Time.deltaTime;
    }*/
}
