using System.Collections;
using UnityEngine;

public class CarroInterativoAR : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float velocidadeRotacao = 0.4f;
    [SerializeField] private float duracaoMortal = 0.8f;
    [SerializeField] private float alturaPulo = 0.5f;
    [SerializeField] private float tempoEsperaReset = 3f;
    [SerializeField] private float velocidadeAutoRotacao = 25f;
    [SerializeField] private float limiteDuploClique = 0.25f;
    [SerializeField] private float limiarArraste = 10f;

    private Vector2 posicaoToqueAnterior;
    private Vector2 posicaoToqueInicial;
    private bool estaFazendoMortal;
    private bool girandoAuto;
    private bool foiArrastado;
    private float tempoInativo;
    private int quantidadeCliques;

    private Vector3 posicaoPadrao;
    private Quaternion rotacaoPadrao;

    private void Start()
    {
        ObterCameraSeNulo();
        posicaoPadrao = transform.localPosition;
        rotacaoPadrao = transform.localRotation;
    }

    private void Update()
    {
        ObterCameraSeNulo();
        ProcessarInteracoes();
        ProcessarEstadoInativoEAutoRotacao();
    }

    private void ObterCameraSeNulo()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    private void ProcessarInteracoes()
    {
        if (estaFazendoMortal) return;

        bool interagiu = false;

        if (Input.touchCount > 0)
        {
            Touch toque = Input.GetTouch(0);

            if (toque.phase == TouchPhase.Began)
            {
                interagiu = true;
                foiArrastado = false;
                posicaoToqueInicial = toque.position;
                posicaoToqueAnterior = toque.position;
                TratarCliqueOuToque(toque.position);
            }
            else if (toque.phase == TouchPhase.Moved)
            {
                interagiu = true;

                if (Vector2.Distance(toque.position, posicaoToqueInicial) > limiarArraste)
                {
                    foiArrastado = true;
                    quantidadeCliques = 0;
                }

                GirarCarro(toque.deltaPosition.x);
            }
            else if (toque.phase == TouchPhase.Stationary)
            {
                interagiu = true;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            interagiu = true;
            foiArrastado = false;
            posicaoToqueInicial = Input.mousePosition;
            posicaoToqueAnterior = Input.mousePosition;
            TratarCliqueOuToque(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            interagiu = true;
            Vector2 mouseAtual = Input.mousePosition;

            if (Vector2.Distance(mouseAtual, posicaoToqueInicial) > limiarArraste)
            {
                foiArrastado = true;
                quantidadeCliques = 0;
            }

            float deltaX = mouseAtual.x - posicaoToqueAnterior.x;
            GirarCarro(deltaX);
            posicaoToqueAnterior = mouseAtual;
        }

        if (interagiu)
        {
            tempoInativo = 0f;
        }
        else
        {
            tempoInativo += Time.deltaTime;
        }
    }

    private void TratarCliqueOuToque(Vector2 telaPosicao)
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(telaPosicao);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                quantidadeCliques++;

                if (quantidadeCliques == 1)
                {
                    StartCoroutine(ProcessarCliqueAguardandoDuplo());
                }
            }
        }
    }

    private IEnumerator ProcessarCliqueAguardandoDuplo()
    {
        yield return new WaitForSeconds(limiteDuploClique);

        if (foiArrastado)
        {
            quantidadeCliques = 0;
            yield break;
        }

        if (quantidadeCliques == 1)
        {
            StartCoroutine(ExecutarMortal());
        }
        else if (quantidadeCliques >= 2)
        {
            girandoAuto = !girandoAuto;
        }

        quantidadeCliques = 0;
    }

    private void GirarCarro(float deltaX)
    {
        transform.Rotate(Vector3.up, -deltaX * velocidadeRotacao, Space.Self);
    }

    private void ProcessarEstadoInativoEAutoRotacao()
    {
        if (estaFazendoMortal) return;

        if (girandoAuto)
        {
            transform.Rotate(Vector3.up, velocidadeAutoRotacao * Time.deltaTime, Space.Self);
        }
        else if (tempoInativo >= tempoEsperaReset)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, posicaoPadrao, Time.deltaTime * 3f);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, rotacaoPadrao, Time.deltaTime * 3f);
        }
    }

    private IEnumerator ExecutarMortal()
    {
        estaFazendoMortal = true;

        Vector3 posicaoInicial = transform.localPosition;
        Quaternion rotacaoInicial = transform.localRotation;

        float tempo = 0f;

        while (tempo < duracaoMortal)
        {
            tempo += Time.deltaTime;
            float progresso = tempo / duracaoMortal;

            float anguloX = Mathf.Lerp(0f, 360f, progresso);
            transform.localRotation = rotacaoInicial * Quaternion.Euler(anguloX, 0f, 0f);

            float alturaAtual = Mathf.Sin(progresso * Mathf.PI) * alturaPulo;
            transform.localPosition = posicaoInicial + new Vector3(0f, alturaAtual, 0f);

            yield return null;
        }

        transform.localPosition = posicaoInicial;
        transform.localRotation = rotacaoInicial;

        estaFazendoMortal = false;
    }
}