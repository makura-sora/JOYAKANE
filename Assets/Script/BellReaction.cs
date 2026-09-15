using UnityEngine;

public class BellReaction : MonoBehaviour
{
    [SerializeField] private Collider _hammer;
    [SerializeField] private float _hitDistance = 0.4f;
    [SerializeField] private float _returnSpeed = 1f;

    private Rigidbody _rb;
    private Vector3 _startPosition;
    private float _offset;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.useGravity = false;

        _startPosition = _rb.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider != _hammer) return;

        // ‰E‘¤‚É“–‚½‚ê‚Î¶‚ÖA¶‘¤‚É“–‚½‚ê‚Î‰E‚Ö
        float hitX = collision.GetContact(0).point.x;
        float direction = hitX >= _rb.position.x ? -1f : 1f;

        _offset = direction * _hitDistance;
    }

    private void FixedUpdate()
    {
        _rb.MovePosition(_startPosition + Vector3.right * _offset);

        _offset = Mathf.MoveTowards(
            _offset, 0f, _returnSpeed * Time.fixedDeltaTime);
    }
}