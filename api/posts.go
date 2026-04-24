package handler

import (
	"encoding/json"
	"net/http"
	"strings"
	"sync"
	"time"
)

type User struct {
	ID       int    `json:"id"`
	Username string `json:"username"`
}

type Post struct {
	ID             int       `json:"id"`
	Text           string    `json:"text"`
	CreatedAt      time.Time `json:"createdAt"`
	Likes          int       `json:"likes"`
	User           User      `json:"user"`
	Hashtags       []string  `json:"hashtags"`
	Mentions       []string  `json:"mentions"`
	CharacterCount int       `json:"characterCount"`
}

var (
	posts       = []Post{}
	defaultUser = User{ID: 1, Username: "GoLangUser"}
	nextID      = 1
	mu          sync.Mutex
)

func extractTags(text string, prefix rune) []string {
	words := strings.Fields(text)
	var tags []string
	for _, w := range words {
		if strings.HasPrefix(w, string(prefix)) && len(w) > 1 {
			tags = append(tags, strings.TrimPrefix(w, string(prefix)))
		}
	}
	return tags
}

func Handler(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")
	w.Header().Set("Access-Control-Allow-Origin", "*")

	if r.Method == "GET" {
		mu.Lock()
		defer mu.Unlock()
		json.NewEncoder(w).Encode(map[string]interface{}{
			"posts":      posts,
			"totalPosts": len(posts),
		})
		return
	}

	if r.Method == "POST" {
		text := r.FormValue("Text")
		if text == "" {
			var body struct {
				Text string `json:"text"`
			}
			json.NewDecoder(r.Body).Decode(&body)
			text = body.Text
		}

		if text == "" {
			http.Error(w, `{"error":"Post text is required."}`, http.StatusBadRequest)
			return
		}

		mu.Lock()
		defer mu.Unlock()

		post := Post{
			ID:             nextID,
			Text:           text,
			CreatedAt:      time.Now(),
			User:           defaultUser,
			Likes:          0,
			Hashtags:       extractTags(text, '#'),
			Mentions:       extractTags(text, '@'),
			CharacterCount: len(text),
		}
		nextID++
		posts = append([]Post{post}, posts...)

		w.WriteHeader(http.StatusCreated)
		json.NewEncoder(w).Encode(post)
		return
	}

	http.Error(w, `{"error":"Method not allowed"}`, http.StatusMethodNotAllowed)
}
