using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Cinemachine;
using Object = UnityEngine.Object;

public static class PA2Validation
{
    static readonly List<string> log = new List<string>();
    static PlayerController p;
    static Rigidbody2D rb;
    static FieldInfo axis = typeof(PlayerController).GetField("horizontal", BindingFlags.NonPublic | BindingFlags.Instance);
    static FieldInfo jump = typeof(PlayerController).GetField("buffer", BindingFlags.NonPublic | BindingFlags.Instance);
    static MethodInfo fixedTick = typeof(PlayerController).GetMethod("FixedUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
    static MethodInfo animationTick = typeof(PlayerAnimation).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
    static void Check(bool valid, string message)
    {
        log.Add((valid ? "PASS " : "FAIL ") + message);
    }
    static void Place(float x, float y)
    {
        rb.position = new Vector2(x, y);
        p.transform.position = new Vector3(x, y);
        rb.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
    }
    static void Step(float direction = 0, bool requestJump = false)
    {
        axis.SetValue(p, direction);
        if (requestJump)
            jump.SetValue(p, .12f);
        fixedTick.Invoke(p, null);
        Physics2D.Simulate(.02f);
        var animation = p.GetComponent<PlayerAnimation>();
        animationTick.Invoke(animation, null);
        animation.animator.Update(.02f);
    }
    static void Settle(int frames = 20)
    {
        for (int i = 0; i < frames; i++)
            Step();
    }
    static void Move(float target, int frames, bool requestJump = false)
    {
        for (int i = 0; i < frames; i++)
            Step(Mathf.Abs(target - rb.position.x) < .07f ? 0 : Mathf.Sign(target - rb.position.x), requestJump && i == 0);
    }
    public static void Route()
    {
        log.Clear();
        p = Object.FindAnyObjectByType<PlayerController>();
        rb = p.GetComponent<Rigidbody2D>();
        var gm = GameManager.Instancia;
        gm.Continuar();
        p.enabled = false;
        var mode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;
        try
        {
            Settle();
            foreach (var leg in new[] { new Vector2(4.8f, 0), new Vector2(8, 1), new Vector2(9, 0), new Vector2(14.5f, 1), new Vector2(15.2f, 0), new Vector2(20.3f, 1), new Vector2(24, 1), new Vector2(25, 0), new Vector2(31, 1), new Vector2(38.2f, 0), new Vector2(43.3f, 1), new Vector2(47, 1), new Vector2(48, 0), new Vector2(54, 1), new Vector2(55, 0), new Vector2(60, 1), new Vector2(62.2f, 0), new Vector2(67.3f, 1), new Vector2(71, 1), new Vector2(72, 0), new Vector2(78, 1), new Vector2(79, 0), new Vector2(84, 0) })
            {
                Move(leg.x, 100, leg.y > 0);
                Settle(8);
                log.Add("Destino " + leg.x + ": posición=" + rb.position + " monedas=" + gm.Monedas + " vidas=" + p.GetComponent<PlayerHealth>().Vidas);
                if (gm.EstadoActual == GameManager.Estado.Derrota)
                    break;
            }
            Check(gm.EstadoActual == GameManager.Estado.Victoria, "Recorrido continuo desde inicio hasta meta");
            Check(p.GetComponent<PlayerHealth>().Vidas == 3, "Recorrido completo sin perder vidas");
        }
        catch (Exception e) { log.Add("ERROR " + e); }
        finally { Physics2D.simulationMode = mode; p.enabled = true; }
        File.WriteAllLines("Temp/pa2-route.txt", log);
    }
    [MenuItem("PA2/Validar sistemas en Play")]
    public static void Run()
    {
        log.Clear();
        try
        {
            if (!EditorApplication.isPlaying)
                throw new InvalidOperationException("Inicia Play para validar.");
            p = Object.FindAnyObjectByType<PlayerController>();
            rb = p.GetComponent<Rigidbody2D>();
            var gm = GameManager.Instancia;
            var hud = Object.FindAnyObjectByType<GameHUD>();
            var health = p.GetComponent<PlayerHealth>();
            Check(gm != null && AudioManager.Instancia != null, "Singletons activos");
            Check(gm.EstadoActual == GameManager.Estado.Inicio && Time.timeScale == 0, "Menú inicial detiene la partida");
            Check(Object.FindObjectsByType<Moneda>().Length == 24, "24 monedas en el nivel");
            Check(Object.FindObjectsByType<Tilemap>().Any(t => t.GetComponent<TilemapCollider2D>() != null && t.GetUsedTilesCount() > 2), "Tilemaps con colisión compuesta");
            var cm = Object.FindAnyObjectByType<CinemachineCamera>();
            Check(cm.Follow == p.transform && cm.GetComponent<CinemachineConfiner2D>().BoundingShape2D != null && cm.GetComponent<CinemachinePositionComposer>().Composition.DeadZone.Enabled, "Cinemachine: seguimiento, zona muerta y límites");
            var ac = p.GetComponent<PlayerAnimation>().animator.runtimeAnimatorController as AnimatorController;
            Check(ac.layers[0].stateMachine.states.Length == 4 && ac.layers[0].stateMachine.anyStateTransitions.All(t => !t.hasExitTime && t.duration == 0), "Animator: Idle, Run, Jump, Fall sin Exit Time");
            Check(Object.FindObjectsByType<Transform>().All(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) == 0), "Sin scripts faltantes en la escena");
            Check(AudioManager.Instancia.musica.clip != null && AudioManager.Instancia.musica.loop && AudioManager.Instancia.clips.All(c => c != null), "Música en bucle y cinco efectos asignados");
            Check(p.GetComponent<PlayerFeedback>().polvo != null && p.GetComponent<PlayerFeedback>().impacto != null, "Partículas de salto e impacto asignadas");
            gm.Continuar();
            p.enabled = false;
            var mode = Physics2D.simulationMode;
            Physics2D.simulationMode = SimulationMode2D.Script;
            foreach (var map in Object.FindObjectsByType<Tilemap>())
                map.RefreshAllTiles();
            foreach (var tileCollider in Object.FindObjectsByType<TilemapCollider2D>())
                tileCollider.ProcessTilemapChanges();
            foreach (var composite in Object.FindObjectsByType<CompositeCollider2D>())
                composite.GenerateGeometry();
            Physics2D.SyncTransforms();
            log.Add("INFO pos=" + rb.position + " gravity=" + Physics2D.gravity + " body=" + rb.bodyType + " simulation=" + rb.simulated);
            foreach (var collider in Object.FindObjectsByType<CompositeCollider2D>())
                log.Add("INFO collider " + collider.name + " paths=" + collider.pathCount + " bounds=" + collider.bounds);
            try
            {
                Settle();
                Check(p.EnSuelo && Mathf.Abs(rb.position.y) < .15f, "Jugador aterriza y detecta el suelo");
                Place(84, .05f);
                Step();
                Check(gm.Jugando && gm.Mensaje.Contains("monedas"), "Meta bloqueada sin 10 monedas");
                Place(1, .05f);
                Settle();
                Move(4.8f, 50);
                Check(rb.position.x > 4.4f, "Movimiento y empuje del barril");
                int before = gm.Monedas;
                Place(3, .2f);
                Step();
                Step();
                Check(gm.Monedas == before, "Moneda recogida no se duplica al volver");
                Place(4.8f, .03f);
                Settle();
                Step(1, true);
                Check(p.VelocidadY > 5, "Salto usa el Rigidbody2D");
                Move(8, 65);
                Settle();
                Check(p.EnSuelo && Mathf.Abs(rb.position.y - 2) < .2f, "Salto alcanza la plataforma elevada de 2 unidades");
                Step(-1);
                Check(p.GetComponent<PlayerAnimation>().imagen.flipX, "Orientación del personaje cambia al moverse");
                foreach (var gap in new[] { new Vector2(15.2f, 20.3f), new Vector2(38.2f, 43.3f), new Vector2(62.2f, 67.3f) })
                {
                    Place(gap.x, .03f);
                    Settle();
                    Move(gap.y, 70, true);
                    Settle();
                    Check(rb.position.x > gap.y - .2f && Mathf.Abs(rb.position.y) < .2f, "Salto jugable sobre hueco desde x=" + gap.x);
                }
                Check(Mathf.Abs(p.PuntoControl.x - 68) < .1f, "Punto de control actualiza la reaparición");
                int lives = health.Vidas;
                Place(27, .05f);
                Step();
                Check(health.Vidas == lives - 1 && Mathf.Abs(rb.position.x - 68) < .5f, "Púas restan una vida y reaparece en el control");
                health.RecibirDano();
                Check(health.Vidas == lives - 1, "Invulnerabilidad evita daño repetido");
                foreach (var coin in Object.FindObjectsByType<Moneda>())
                {
                    if (gm.Monedas >= 10)
                        break;
                    var v = coin.transform.position;
                    Place(v.x, v.y - .5f);
                    Step();
                    Step();
                }
                Check(gm.Monedas >= 10 && hud.monedas.text.Contains(gm.Monedas.ToString("00")), "Colección real por colisión actualiza HUD mediante eventos");
                gm.Pausar();
                Check(Time.timeScale == 0 && hud.panel.activeSelf, "Pausa congela la partida y muestra el panel");
                gm.Continuar();
                Check(Time.timeScale == 1 && !hud.panel.activeSelf, "Continuar reanuda la partida");
                AudioManager.Instancia.AlternarSonido();
                Check(AudioManager.Instancia.musica.mute && AudioManager.Instancia.efectos.mute, "Control de sonido silencia música y efectos");
                AudioManager.Instancia.AlternarSonido();
                Place(84, .05f);
                Step();
                Check(gm.EstadoActual == GameManager.Estado.Victoria && hud.panel.activeSelf && Time.timeScale == 0, "Meta produce victoria y bloquea el movimiento");
                gm.Continuar();
                while (health.Vidas > 0)
                    health.RecibirDano(true);
                Check(gm.EstadoActual == GameManager.Estado.Derrota && Time.timeScale == 0, "Agotar vidas produce derrota");
            }
            finally { Physics2D.simulationMode = mode; p.enabled = true; }
        }
        catch (Exception e) { log.Add("ERROR " + e); }
        File.WriteAllLines("Temp/pa2-validation.txt", log);
        Debug.Log(string.Join("\n", log));
    }
}
